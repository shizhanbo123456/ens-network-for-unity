using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static KeyLibrary;


/// <summary>
/// NOMBehaviour提供了函数调用同步的功能
/// 调用CallFuncServerRpc即可实现同步调用<br></br>
/// 客户端主动调用的方法传入一个string类型的param，被调用时传入原来的string
/// 初始存在的物体需要enabled=true，销毁物体使用DestroyRpc或DestroyLocal
/// </summary>
public abstract class EnsBehaviour : MonoBehaviour
{
    // <0为玩家设置的初始化场景时制造的物体  >0为游戏过程中制造的物体的Id  =0为未分配的
    public int ObjectId=0;
    public bool nomEnabled = true;

    internal EnsBehaviourCollection collection;


    private static readonly Dictionary<KeyFormatType, string> Key2Header = new Dictionary<KeyFormatType, string>() 
    { 
        {KeyFormatType.None,Header.F },
        {KeyFormatType.Nonsequential,Header.kF },
        {KeyFormatType.Timewise,Header.KF }
    };
    public enum SendTo
    {
        Everyone=-1,
        ExcludeSender=-2,
        RoomOwner=-3
    }

    internal bool IdAutoAllocated=false;
    private bool startInvoked=false;

    protected void Start()
    {
        NOMStart();
        _Start();
    }
    internal void NOMStart()
    {
        if(startInvoked) return;
        startInvoked = true;
        if (ObjectId == 0)
        {
            ObjectId = EnsNetworkObjectManager.GetAutoAllocatedId();
            IdAutoAllocated = true;
        }
        else
        {
            if (!IdAutoAllocated)
            {
                int id = ObjectId % 100000000 + 2000000000;
                if (EnsNetworkObjectManager.ManualAssignedId.Contains(id))
                {
                    if(EnsNetworkObjectManager.HasObject(id))
                        Debug.LogError("手动分配的id发生冲突");
                }
                else
                {
                    EnsNetworkObjectManager.ManualAssignedId.Add(id);
                }
            }
        }
        EnsNetworkObjectManager.AddObject(this);
    }
    protected virtual void _Start()
    {
        
    }
    public void DestroyRpc(KeyFormatType keyFormatType=KeyFormatType.Nonsequential)
    {
        CallFuncRpc(nameof(DestroyLocal), SendTo.Everyone, keyFormatType);
    }
    public void DestroyLocal()
    {
        //foreach (var i in collection.Behaviors) i.NOMOnDestroy();//物体管理器移除物体+人为分配id移除物体
        if (collection != null) Destroy(collection.gameObject);
    }
    /// <summary>
    /// 需要发送数据时，在此Update中使用，减少调用到传输的延迟<br></br>
    /// </summary>
    public virtual void ManagedUpdate()
    {

    }
    /// <summary>
    /// 需要发送数据时，在此FixedUpdate中使用，减少调用到传输的延迟<br></br>
    /// </summary>
    public virtual void FixedManagedUpdate()
    {

    }

    public void CallFuncRpc(string func, SendTo mode, KeyFormatType type=KeyFormatType.None )
    {
        if (EnsInstance.Corr.networkMode == EnsCorrespondent.NetworkMode.None)
        {
            if (mode != SendTo.ExcludeSender) StartCoroutine(func);
            return;
        }
        EnsInstance.Corr.Client.SendData(Key2Header[type] + ObjectId.ToString() + "#{" + func + "}#" + (int)mode);
    }
    public void CallFuncRpc(string func,List<int> targets, KeyFormatType type = KeyFormatType.None)
    {
        if (targets.Count == 0) return;
        if (EnsInstance.Corr.networkMode == EnsCorrespondent.NetworkMode.None)
        {
            if (targets.Contains(EnsInstance.LocalClientId)) StartCoroutine(func);
            return;
        }
        EnsInstance.Corr.Client.SendData(Key2Header[type] + ObjectId.ToString() + "#{" + func + "}#" + Format.ListToString(targets));
    }
    public void CallFuncRpc(string func, SendTo mode,string param, KeyFormatType type = KeyFormatType.None)
    {
        if (EnsInstance.Corr.networkMode == EnsCorrespondent.NetworkMode.None)
        {
            if (mode != SendTo.ExcludeSender) StartCoroutine(func,param);
            return;
        }
        EnsInstance.Corr.Client.SendData(Key2Header[type] + ObjectId.ToString() + "#{" + func + "}#" + (int)mode + "#{" + param+'}');
    }
    public void CallFuncRpc(string func, List<int> targets,string param, KeyFormatType type = KeyFormatType.None)
    {
        if (targets.Count == 0) return;
        if (EnsInstance.Corr.networkMode == EnsCorrespondent.NetworkMode.None)
        {
            if (targets.Contains(EnsInstance.LocalClientId)) StartCoroutine(func, param);
            return;
        }
        EnsInstance.Corr.Client.SendData(Key2Header[type] + ObjectId.ToString() + "#{" + func + "}#" + Format.ListToString(targets) + "#{" + param+'}');
    }
    public void CallFuncRpc(string func, SendTo mode, int delay)
    {
        if (EnsInstance.Corr.networkMode == EnsCorrespondent.NetworkMode.None)
        {
            if (mode != SendTo.ExcludeSender) StartCoroutine(func);
            return;
        }
        EnsInstance.Corr.Client.SendData(Header.kS + ObjectId.ToString() + "#{" + func+ "}#" + (int)mode + "#" + delay);
    }
    public void CallFuncRpc(string func, List<int> targets, int delay)
    {
        if (targets.Count == 0) return;
        if (EnsInstance.Corr.networkMode == EnsCorrespondent.NetworkMode.None)
        {
            if (targets.Contains(EnsInstance.LocalClientId)) StartCoroutine(func);
            return;
        }
        EnsInstance.Corr.Client.SendData(Header.kS + ObjectId.ToString() + "#{" + func + "}#" + Format.ListToString(targets) + "#" + delay);
    }
    public void CallFuncRpc(string func, SendTo mode, string param, int delay)
    {
        if (EnsInstance.Corr.networkMode == EnsCorrespondent.NetworkMode.None)
        {
            if (mode != SendTo.ExcludeSender) StartCoroutine(func, param);
            return;
        }
        EnsInstance.Corr.Client.SendData(Header.kS + ObjectId.ToString() + "#{" + func + "}#" + (int)mode + "#" + delay + "#{" + param+'}');
    }
    public void CallFuncRpc(string func, List<int> targets, string param, int delay)
    {
        if (targets.Count == 0) return;
        if (EnsInstance.Corr.networkMode == EnsCorrespondent.NetworkMode.None)
        {
            if (targets.Contains(EnsInstance.LocalClientId)) StartCoroutine(func, param);
            return;
        }
        EnsInstance.Corr.Client.SendData(Header.kS + ObjectId.ToString() + "#{" + func + "}#" + Format.ListToString(targets) + "#" + delay + "#{" + param+'}');
    }


    internal void CallFuncLocal(string func)
    {
        StartCoroutine(func);
    }
    internal void CallFuncLocal(string func, string param)
    {
        if (gameObject.activeSelf)
        {
            StartCoroutine(func, param);
        }
    }
    internal void DelayInvoke(List<string> s)
    {
        StartCoroutine(WaitForInvoke(s));
    }
    private IEnumerator WaitForInvoke(List<string> s)
    {
        var delay = int.Parse(s[3]);
        if(delay>0)yield return new WaitForSeconds(delay / 1000f);
        if (s.Count >= 5)
        {
            CallFuncLocal(s[1], s[4]);
        }
        else
        {
            CallFuncLocal(s[1]);
        }
    }
}
