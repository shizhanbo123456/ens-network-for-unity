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
public abstract class NOMBehaviour : MonoBehaviour
{
    // <0为玩家设置的初始化场景时制造的物体  >0为游戏过程中制造的物体的Id  =0为未分配的
    public int ObjectId=0;
    public bool nomEnabled = true;

    internal NOMBehaviourCollection collection;

    //FuncInvokeMode All -1,IgnoreSelf -2,Custom

    private static readonly Dictionary<KeyFormatType, char> KeyTypeToChar = new Dictionary<KeyFormatType, char>() 
    { 
        {KeyFormatType.None,'[' },
        {KeyFormatType.Nonsequential,'k' },
        {KeyFormatType.Timewise,'K' }
    };
    private static List<char> InvalidParam => new List<char>()
    {
        '*','#','[',']',NOMInstance.TargetSeparator
    };

    internal bool internalAllocateId=false;
    private bool startInvoked=false;
    private bool destroyInvoked=false;

    private void Start()
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
            ObjectId = NOMInstance.AutoSceneObjId;
            internalAllocateId = true;
            if (NOMInstance.LogOnAutoAssignedId) Debug.Log(gameObject.name + "已被自动分配id:" + ObjectId);
        }
        else
        {
            if (!internalAllocateId)
            {
                int id = ObjectId % 100000000 + 2000000000;
                if (NOMInstance.ManualAssignedId.Contains(id))
                {
                    Debug.LogError("手动分配的id发生冲突");
                }
                else
                {
                    NOMInstance.ManualAssignedId.Add(id);
                }
            }
        }
        if (!NOMInstance.HasObject(ObjectId)) NOMInstance.AddObject(this);
    }
    protected virtual void _Start()
    {
        
    }
    protected void DisableInternalAllocatedId()
    {
        if (internalAllocateId) Debug.LogError(gameObject.name+"不应该使用自动分配的id，请手动分配");
    }
    public void DestroyRpc(KeyFormatType keyFormatType=KeyFormatType.Nonsequential)
    {
        CallFuncRpc(nameof(DestroyLocal), -1, keyFormatType);
    }
    public void DestroyLocal()
    {
        try
        {
            foreach (var i in collection.Behaviors) i.NOMOnDestroy();
            if (this != null) Destroy(gameObject);
        }
        catch
        {
            Debug.LogError("似乎意外直接销毁了一个网络物体");
        }
    }
    private void NOMOnDestroy()
    {
        if (destroyInvoked) return;
        destroyInvoked = true;
        if(NOMInstance.HasObject(ObjectId))NOMInstance.RemoveObject(ObjectId);
        if(!internalAllocateId)NOMInstance.ManualAssignedId.Remove(ObjectId);
    }
    /// <summary>
    /// 需要发送数据时，在此Update中使用，从而便于将数据合并发送<br></br>
    /// 在发送数据前调用
    /// </summary>
    public virtual void ManagedUpdate()
    {

    }
    public virtual void FixedManagedUpdate()
    {

    }

    public void CallFuncRpc(string func, int mode, KeyFormatType type=KeyFormatType.None )
    {
        if (ENCInstance.DevelopmentDebug)
        {
            if (mode != -1 && mode != -2)
            {
                Debug.LogError("检测到非法访问目标:" + mode);
                mode = -1;
            }
        }
        if (NOMInstance.NOMCorrespondent.networkMode == ENCCorrespondent.NetworkMode.None)
        {
            if (mode != -2) StartCoroutine(func);
            return;
        }
        char s = KeyTypeToChar[type];
        NOMInstance.NOMCorrespondent.ENCClient.SendData(s+"F]" + ObjectId.ToString() + "#" + func + "#" + mode);
    }
    public void CallFuncRpc(string func,List<int> targets, KeyFormatType type = KeyFormatType.None)
    {
        if (NOMInstance.NOMCorrespondent.networkMode == ENCCorrespondent.NetworkMode.None)
        {
            if (targets.Contains(ENCInstance.LocalClientId)) StartCoroutine(func);
            return;
        }
        char s = KeyTypeToChar[type];
        NOMInstance.NOMCorrespondent.ENCClient.SendData(s+"F]" + ObjectId.ToString() + "#" + func + "#" + Format.ListToString(targets,NOMInstance.TargetSeparator));
    }
    public void CallFuncRpc(string func, int mode,string param, KeyFormatType type = KeyFormatType.None)
    {
        if (ENCInstance.DevelopmentDebug)
        {
            foreach(var i in InvalidParam)
            {
                if (param.Contains(i))
                {
                    Debug.LogError($"参数包含非法字符:{param}");
                }
            }
            if (mode != -1 && mode != -2)
            {
                Debug.LogError("检测到非法访问目标:" + mode);
                mode = -1;
            }
        }
        if (NOMInstance.NOMCorrespondent.networkMode == ENCCorrespondent.NetworkMode.None)
        {
            if (mode != -2) StartCoroutine(func,param);
            return;
        }
        char s = KeyTypeToChar[type];
        NOMInstance.NOMCorrespondent.ENCClient.SendData(s+"F]" + ObjectId.ToString() + "#" + func + "#" + mode + "#" + param);
    }
    public void CallFuncRpc(string func, List<int> targets,string param, KeyFormatType type = KeyFormatType.None)
    {
        if (ENCInstance.DevelopmentDebug)
        {
            foreach (var i in InvalidParam)
            {
                if (param.Contains(i))
                {
                    Debug.LogError($"参数包含非法字符:{param}");
                }
            }
        }
        if (NOMInstance.NOMCorrespondent.networkMode == ENCCorrespondent.NetworkMode.None)
        {
            if (targets.Contains(ENCInstance.LocalClientId)) StartCoroutine(func, param);
            return;
        }
        char s = KeyTypeToChar[type];
        NOMInstance.NOMCorrespondent.ENCClient.SendData(s+"F]" + ObjectId.ToString() + "#" + func + "#" + Format.ListToString(targets, NOMInstance.TargetSeparator) + "#" + param);
    }
    public void CallFuncRpc(string func, int mode, bool fastinvoke)
    {
        if (ENCInstance.DevelopmentDebug)
        {
            if (!fastinvoke)
            {
                Debug.LogWarning("请考虑使用Key传输");
            }
            if (mode != -1 && mode != -2)
            {
                Debug.LogError("检测到非法访问目标:" + mode);
                mode = -1;
            }
        }
        if (NOMInstance.NOMCorrespondent.networkMode == ENCCorrespondent.NetworkMode.None)
        {
            if (mode != -2) StartCoroutine(func);
            return;
        }
        string header = fastinvoke ? "iF]" : "[F]";
        NOMInstance.NOMCorrespondent.ENCClient.SendData(header + ObjectId.ToString() + "#" + func + "#" + mode);
    }
    public void CallFuncRpc(string func, List<int> targets, bool fastinvoke)
    {
        if (ENCInstance.DevelopmentDebug)
        {
            if (!fastinvoke)
            {
                Debug.LogWarning("请考虑使用Key传输");
            }
        }
        if (NOMInstance.NOMCorrespondent.networkMode == ENCCorrespondent.NetworkMode.None)
        {
            if (targets.Contains(ENCInstance.LocalClientId)) StartCoroutine(func);
            return;
        }
        string header = fastinvoke ? "iF]" : "[F]";
        NOMInstance.NOMCorrespondent.ENCClient.SendData(header + ObjectId.ToString() + "#" + func + "#" + Format.ListToString(targets, NOMInstance.TargetSeparator));
    }
    public void CallFuncRpc(string func, int mode, string param, bool fastinvoke)
    {
        if (ENCInstance.DevelopmentDebug)
        {
            foreach (var i in InvalidParam)
            {
                if (param.Contains(i))
                {
                    Debug.LogError($"参数包含非法字符:{param}");
                }
            }
            if (!fastinvoke)
            {
                Debug.LogWarning("请考虑使用Key传输");
            }
            if (mode != -1 && mode != -2)
            {
                Debug.LogError("检测到非法访问目标:" + mode);
                mode = -1;
            }
        }
        if (NOMInstance.NOMCorrespondent.networkMode == ENCCorrespondent.NetworkMode.None)
        {
            if (mode != -2) StartCoroutine(func, param);
            return;
        }
        string header = fastinvoke ? "iF]" : "[F]";
        NOMInstance.NOMCorrespondent.ENCClient.SendData(header + ObjectId.ToString() + "#" + func + "#" + mode + "#" + param);
    }
    public void CallFuncRpc(string func, List<int> targets, string param, bool fastinvoke)
    {
        if (ENCInstance.DevelopmentDebug)
        {
            foreach (var i in InvalidParam)
            {
                if (param.Contains(i))
                {
                    Debug.LogError($"参数包含非法字符:{param}");
                }
            }
            if (!fastinvoke)
            {
                Debug.LogWarning("请考虑使用Key传输");
            }
        }
        if (NOMInstance.NOMCorrespondent.networkMode == ENCCorrespondent.NetworkMode.None)
        {
            if (targets.Contains(ENCInstance.LocalClientId)) StartCoroutine(func, param);
            return;
        }
        string header = fastinvoke ? "iF]" : "[F]";
        NOMInstance.NOMCorrespondent.ENCClient.SendData(header + ObjectId.ToString() + "#" + func + "#" + Format.ListToString(targets, NOMInstance.TargetSeparator) + "#" + param);
    }
    public void CallFuncRpc(string func, int mode, int delay)
    {
        if (ENCInstance.DevelopmentDebug)
        {
            if (mode != -1 && mode != -2)
            {
                Debug.LogError("检测到非法访问目标:" + mode);
                mode = -1;
            }
        }
        if (NOMInstance.NOMCorrespondent.networkMode == ENCCorrespondent.NetworkMode.None)
        {
            if (mode != -2) StartCoroutine(func);
            return;
        }
        NOMInstance.NOMCorrespondent.ENCClient.SendData("kS]" + ObjectId.ToString() + "#" + func+ "#" + mode + "#" + delay);
    }
    public void CallFuncRpc(string func, List<int> targets, int delay)
    {
        if (NOMInstance.NOMCorrespondent.networkMode == ENCCorrespondent.NetworkMode.None)
        {
            if (targets.Contains(ENCInstance.LocalClientId)) StartCoroutine(func);
            return;
        }
        NOMInstance.NOMCorrespondent.ENCClient.SendData("kS]" + ObjectId.ToString() + "#" + func + "#" + Format.ListToString(targets, NOMInstance.TargetSeparator) + "#" + delay);
    }
    public void CallFuncRpc(string func, int mode, string param, int delay)
    {
        if (ENCInstance.DevelopmentDebug)
        {
            foreach (var i in InvalidParam)
            {
                if (param.Contains(i))
                {
                    Debug.LogError($"参数包含非法字符:{param}");
                }
            }
            if (mode != -1 && mode != -2)
            {
                Debug.LogError("检测到非法访问目标:" + mode);
                mode = -1;
            }
        }
        if (NOMInstance.NOMCorrespondent.networkMode == ENCCorrespondent.NetworkMode.None)
        {
            if (mode != -2) StartCoroutine(func, param);
            return;
        }
        NOMInstance.NOMCorrespondent.ENCClient.SendData("kS]" + ObjectId.ToString() + "#" + func + "#" + mode + "#" + delay + "#" + param);
    }
    public void CallFuncRpc(string func, List<int> targets, string param, int delay)
    {
        if (ENCInstance.DevelopmentDebug)
        {
            foreach (var i in InvalidParam)
            {
                if (param.Contains(i))
                {
                    Debug.LogError($"参数包含非法字符:{param}");
                }
            }
        }
        if (NOMInstance.NOMCorrespondent.networkMode == ENCCorrespondent.NetworkMode.None)
        {
            if (targets.Contains(ENCInstance.LocalClientId)) StartCoroutine(func, param);
            return;
        }
        NOMInstance.NOMCorrespondent.ENCClient.SendData("kS]" + ObjectId.ToString() + "#" + func + "#" + Format.ListToString(targets, NOMInstance.TargetSeparator) + "#" + delay + "#" + param);
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
    internal void DelayInvoke(string[] s)
    {
        StartCoroutine(WaitForInvoke(s));
    }
    private IEnumerator WaitForInvoke(string[] s)
    {
        var delay = int.Parse(s[3]);
        if(delay>0)yield return new WaitForSeconds(delay / 1000f);
        if (s.Length >= 5)
        {
            CallFuncLocal(s[1], s[4]);
        }
        else
        {
            CallFuncLocal(s[1]);
        }
    }
}
