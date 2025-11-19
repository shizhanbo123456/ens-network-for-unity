using System;
using System.Collections.Generic;
using Utils;

/// <summary>
/// Add OnReceive Update Clear
/// </summary>
public class KeyLibrary//处理的是未经NetcodeTool格式处理的信息，即"占用"中的信息
{
    public enum KeyFormatType
    {
        None,
        Nonsequential,//k
        Timewise//K
    }
    private List<ENCKey> Keys = new List<ENCKey>();
    private List<ENCKey> TimeWiseKeys = new List<ENCKey>();
    private List<ENCRKey> RecvKeys = new List<ENCRKey>();

    private CircularQueue<string> Response = new CircularQueue<string>();//储存的格式：[K112233][F]....
    private CircularQueue<string> AddBuffer= new CircularQueue<string>();

    private static void UpdateEvent(ENCKey k, List<string>r)
    {
        switch (k.State)
        {
            case ENCKey.KeyState.TobeConfirmed:
                {
                    if (k.ConfirmedExistingSource.Reached)
                    {
                        k.State = ENCKey.KeyState.End;
                    }

                    if (k.ToConfirmIntervalLeft.Reached)
                    {
                        k.ToConfirmIntervalLeft.ReachAfter(EnsInstance.KeySendInterval);

                        string res;
                        if (k.Type == KeyFormatType.Nonsequential) res = "[k" + k.Index + "]" + k.Key;
                        else res = "[K" + k.Index + "]" + k.Key;
                        r.Add(res);
                    }
                    break;
                }
            case ENCKey.KeyState.ConfirmedExisting:
                {
                    if (k.ConfirmedExistingSource.Reached) k.State = ENCKey.KeyState.End;
                    break;
                }
        }
    }
    private static bool RecvEvent(ENCKey k)
    {
        bool skip = false;
        if (k.State == ENCKey.KeyState.ConfirmedExisting) skip = true;
        else if (k.State == ENCKey.KeyState.TobeConfirmed)
        {
            k.State = ENCKey.KeyState.ConfirmedExisting;
            skip = true;
        }
        return skip;
    }

    public static string Format(string data,KeyFormatType type)
    {
        if (type == KeyFormatType.None)
        {
            if (data[0]=='[') Debug.Log("格式错误");
            return '[' + data.Substring(1, data.Length - 1);
        }
        if (type == KeyFormatType.Nonsequential)
        {
            if (data[0] == 'k') Debug.Log("格式错误");
            return 'k' + data.Substring(1, data.Length - 1);
        }
        if (type == KeyFormatType.Timewise)
        {
            if (data[0] == 'K') Debug.Log("格式错误");
            return 'K' + data.Substring(1, data.Length - 1);
        }
        Debug.LogError("未处理的关键类型");
        return string.Empty;
    }


    /// <summary>
    /// 程序内部传输，将需要进行检查的信息第一位'['改为'K'<br></br>
    /// 识别一个数据释放要进行检查的唯一依据就是data[0]=='K'<br></br>
    /// 传入KF]....，传入[K112233][F]....
    /// </summary>
    public void Add(string data)//输入  KF].... kF]....
    {
        AddBuffer.Write(data);
    }
    /// <summary>
    /// 处理所有标记为K的信息<br></br>
    /// 本地无标记则发送回应确认收到<br></br>
    /// 传入[K112233][F]....，传出 KF]....
    /// </summary>
    public void OnRecvData(string data, out bool skip, out string t_data)//传入[K112233][F].... [k112233][F]....
    {
        skip = false;
        t_data = string.Empty;
        if (data[1] != 'K'&& data[1] != 'k') return;

        int indexEnd = 1;
        while (data[indexEnd] != ']' && indexEnd < data.Length - 1) indexEnd++;
        if (data[indexEnd] != ']') return;
        int index = int.Parse(data.Substring(2, indexEnd - 2));//[K112233]abc

        t_data = data.Substring(indexEnd + 1, data.Length - indexEnd - 1);
        if (t_data[1]=='K')t_data=Format(t_data,KeyFormatType.Timewise);
        else t_data = Format(t_data, KeyFormatType.Nonsequential);


        //本地//////////////////////////////////////////////////////////////
        foreach (var k in Keys)
        {
            if (k.Index == index)
            {
                skip=RecvEvent(k);
                return;
            }
        }
        foreach (var k in TimeWiseKeys)
        {
            if (k.Index == index)
            {
                skip = RecvEvent(k);
                return;
            }
        }
        ////////////////////////////////////////////////////////////////////
        ///

        //非本地//////////////////////////////////////////////////////////////////
        Response.Write(data);
        foreach (var k in RecvKeys)
        {
            if (index == k.Index)
            {
                skip = true;
                break;
            }
        }
        if (!skip)
        {
            RecvKeys.Add(new ENCRKey(t_data, index));
        }
    }
    public List<string> Update()//传出[K112233][F]....
    {
        while(AddBuffer.Read(out var data))
        {
            data = Format(data,KeyFormatType.None);
            if (data[0]=='k')Keys.Add(new ENCKey(data,KeyFormatType.Nonsequential));
            else TimeWiseKeys.Add(new ENCKey(data, KeyFormatType.Timewise));
        }

        for (int i = RecvKeys.Count - 1; i >= 0; i--)
        {
            if (RecvKeys[i].cancel.Reached) RecvKeys.RemoveAt(i);
        }
        List<string> r = new List<string>();
        foreach (var k in Keys) UpdateEvent(k, r);
        if (TimeWiseKeys.Count > 0)
        {
            int index = 0;
            while (index < TimeWiseKeys.Count&&TimeWiseKeys[index].State != ENCKey.KeyState.TobeConfirmed) index++;

            for (int i = Math.Min(index,TimeWiseKeys.Count-1); i >= 0; i--) UpdateEvent(TimeWiseKeys[i], r);
        }
        for(int i = Keys.Count - 1; i >= 0; i--) if (Keys[i].State == ENCKey.KeyState.End) Keys.RemoveAt(i);
        for (int i = TimeWiseKeys.Count - 1; i >= 0; i--) if (TimeWiseKeys[i].State == ENCKey.KeyState.End) TimeWiseKeys.RemoveAt(i);

        while (Response.Read(out var d)) r.Add(d);
        return r;
    }
    public void Clear()
    {
        Keys.Clear();
        TimeWiseKeys.Clear();
    }
}
public class ENCKey
{
    private static Random Random = new Random();
    public static int IndexRange = 100000000;
    public string Key;
    public int Index;
    public KeyLibrary.KeyFormatType Type;
    public enum KeyState
    {
        TobeConfirmed, ConfirmedExisting, End
    }
    public KeyState State = KeyState.TobeConfirmed;
    public ReachTime ToConfirmIntervalLeft;
    public ReachTime ConfirmedExistingSource;

    public ENCKey(string data, KeyLibrary.KeyFormatType type)
    {
        ToConfirmIntervalLeft = new ReachTime();
        ToConfirmIntervalLeft.ReachAt(-1f);
        ConfirmedExistingSource = new ReachTime(EnsInstance.KeyExistTime, ReachTime.InitTimeFlagType.ReachAfter);
        Key = data;
        Index = Random.Next(IndexRange, IndexRange * 10 - 1);
        Type = type;
    }
}
public class ENCRKey//仅用于记录收到的信息
{
    public string Key;
    public int Index;
    public ReachTime cancel;
    public ENCRKey(string data, int index)
    {
        Key = data;
        Index = index;
        cancel= new ReachTime(EnsInstance.RKeyExistTime,ReachTime.InitTimeFlagType.ReachAfter);
    }
}