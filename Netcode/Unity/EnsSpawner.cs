using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static KeyLibrary;

/// <summary>
/// 调用CreateServerRpc制造物体
/// 此类已经继承了NOMBehaviour来同步制造函数
/// </summary>
public sealed class EnsSpawner : EnsBehaviour
{
    [Space]
    //public List<NOMBehaviourCollection> prefabs = new();
    public List<EnsBehaviourCollection> Prefabs = new List<EnsBehaviourCollection>();
    public Vector3 DefaultSpawnPosition = new Vector3(0, -10000, 0);

    private static readonly Dictionary<KeyFormatType, string> Key2Header = new Dictionary<KeyFormatType, string>()
    {
        {KeyFormatType.None,Header.f },
        {KeyFormatType.Nonsequential,Header.kf },
        {KeyFormatType.Timewise,Header.Kf }
    };

    private static int prefabid = 1000000000;
    private static int SingleModeCreatedId=0;

    private void Awake()
    {
        EnsInstance.NOMSpawner=this;

        for (int i = 0; i < Prefabs.Count; i++)
        {
            Prefabs[i].NOMCollectionId = prefabid;
            prefabid++;
        }
    }

    private void CallFuncServrRpc(string func, int mode, string param,int id, KeyFormatType type = KeyFormatType.None)
    {
        if (EnsInstance.Corr.networkMode == EnsCorrespondent.NetworkMode.None)
        {
            if (mode != -2) 
            { 
                Create(param, SingleModeCreatedId); 
                SingleModeCreatedId += GetBehaviourCount(id);
            }
            return;
        }
        EnsInstance.Corr.Client.SendData(Key2Header[type] + ObjectId.ToString() + "#{" + func + "}#" + mode + "#{" + param+"}#"+GetBehaviourCount(id));
    }
    private void CallFuncServrRpc(string func, List<int> targets, string param,int id, KeyFormatType type = KeyFormatType.None)
    {
        if (EnsInstance.Corr.networkMode == EnsCorrespondent.NetworkMode.None)
        {
            if (targets.Contains(EnsInstance.LocalClientId)) 
            { 
                Create(param, SingleModeCreatedId); 
                SingleModeCreatedId += GetBehaviourCount(id); 
            }
            return;
        }
        EnsInstance.Corr.Client.SendData(Key2Header[type] + ObjectId.ToString() + "#{" + func + "}#" + Format.ListToString(targets) + "#{" + param + "}#" + GetBehaviourCount(id));
    }
    private int GetBehaviourCount(int id)
    {
        foreach(var i in Prefabs)
        {
            if(i.NOMCollectionId == id)
            {
                return i.Count;
            }
        }
        throw new System.Exception("[E]无对应的物体");
    }

    /// <param name="mode">==-1 All   ==-2 IgnoreSelf</param>
    public void CreateServerRpc(int id, int mode, KeyFormatType type = KeyFormatType.None)
    {
        if (EnsInstance.DevelopmentDebug)
        {
            if (mode != -1 && mode != -2)
            {
                Debug.LogError("检测到非法访问目标:" + mode);
                mode = -1;
            }
        }
        CallFuncServrRpc(nameof(CreateLocal), mode, id.ToString(),id,type);//会处理未联网时的状态
    }
    /// <summary>
    /// 此方法在非联网情况下无法运作
    /// </summary>
    public void CreateServerRpc(int id, List<int> targets, KeyFormatType type = KeyFormatType.None)
    {
        CallFuncServrRpc(nameof(CreateLocal), targets, id.ToString(),id,type);
    }
    ///<summary>
    ///可以通过此方法在其被创建时立即传入参数
    ///</summary>
    /// <param name="mode">==-1 All   ==-2 IgnoreSelf</param>
    public void CreateServerRpc(int id, int mode,string param, KeyFormatType type = KeyFormatType.None)
    {
        if (EnsInstance.DevelopmentDebug)
        {
            if (mode != -1 && mode != -2)
            {
                Debug.LogError("检测到非法访问目标:" + mode);
                mode = -1;
            }
        }
        CallFuncServrRpc(nameof(CreateLocalP), mode, id.ToString()+'%'+param,id,type);//会处理未联网时的状态
    }
    /// <summary>
    /// 此方法在非联网情况下无法运作<br></br>
    /// 可以通过此方法在其被创建时立即传入参数
    /// </summary>
    public void CreateServerRpc(int id, List<int> targets,string param, KeyFormatType type = KeyFormatType.None)
    {
        CallFuncServrRpc(nameof(CreateLocalP), targets, id.ToString()+'%'+param,id,type);
    }



    internal void Create(string data,int idStart)
    {
        if (HasParam(data))
        {
            CreateLocalP(data, idStart);
        }
        else
        {
            CreateLocal(data, idStart);
        }
    }
    private bool HasParam(string data)
    {
        bool param = false;
        foreach (var i in data)
        {
            if (i == '%')
            {
                param = true;
                break;
            }
        }
        return param;
    }

    /// <summary>
    /// 无参数
    /// </summary>
    private void CreateLocal(string data,int idStart)
    {
        int id = int.Parse(data);
        for (int i = 0; i < Prefabs.Count; i++)
        {
            if (Prefabs[i].NOMCollectionId == id)
            {
                GameObject obj = Instantiate(Prefabs[i].gameObject);
                obj.transform.position = DefaultSpawnPosition;
                obj.GetComponent<EnsBehaviourCollection>().AllocateId(idStart);

                return;
            }
        }
        Debug.LogError("[N]未找到目标物体");
    }
    /// <summary>
    /// 有参数
    /// </summary>
    private void CreateLocalP(string data,int idStart)
    {
        string[] s = data.Split('%');
        int id = int.Parse(s[0]);
        for (int i = 0; i < Prefabs.Count; i++)
        {
            if (Prefabs[i].NOMCollectionId == id)
            {
                GameObject obj = Instantiate(Prefabs[i].gameObject);
                obj.transform.position = DefaultSpawnPosition;
                var c = obj.GetComponent<EnsBehaviourCollection>();
                c.AllocateId(idStart);
                c.PreInit(s[1]);

                return;
            }
        }
        Debug.LogError("[N]未找到目标物体");
    }


    /// <summary>
    /// 对所有客户端执行（已经生成的不会执行）
    /// </summary>
    public void RespawnCheckServerRpc(EnsBehaviourCollection collection, string param, KeyFormatType type = KeyFormatType.None)
    {
        CallFuncRpc(nameof(RespawnCheckLocal), -1, collection.NOMCollectionId + "%" + collection.Behaviors[0].ObjectId
            + '%' + param + "%" + collection.Behaviors[0].ObjectId,type);
    }
    
    private void RespawnCheckLocal(string data)
    {
        string[] s = data.Split('%');
        int check = int.Parse(s[3]);
        if (EnsNetworkObjectManager.HasObject(check)) return;
        int id = int.Parse(s[0]);
        for (int i = 0; i < Prefabs.Count; i++)
        {
            if (Prefabs[i].NOMCollectionId == id)
            {
                GameObject obj = Instantiate(Prefabs[i].gameObject);
                obj.transform.position = DefaultSpawnPosition;
                var c = obj.GetComponent<EnsBehaviourCollection>();
                c.AllocateId(int.Parse(s[1]));
                c.PreRespawn(s[2]);


                break;
            }
        }
    }
}
