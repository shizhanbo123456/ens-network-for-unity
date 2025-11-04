using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于管理预制体中的行为脚本
/// </summary>
public class NOMBehaviourCollection : MonoBehaviour
{
    public int NOMCollectionId;
    [Space]
    public List<NOMBehaviour> Behaviors = new List<NOMBehaviour>();
    public int Count
    {
        get
        {
            return Behaviors.Count;
        }
    }

    internal void AllocateId(int idstart)
    {
        for(int i=0;i<Behaviors.Count;i++)
        {
            Behaviors[i].collection=this;
            Behaviors[i].ObjectId=idstart;
            Behaviors[i].internalAllocateId = true;
            idstart++;
            if (NOMInstance.LogOnAllocateId) Debug.Log("[N]分配预制体Id" + Behaviors[i].ObjectId);
            NOMInstance.AddObject(Behaviors[i]);
        }
    }
    public void PreInit(string data)
    {
        foreach (var i in Behaviors) i.NOMStart();
        Init(data);
    }
    protected virtual void Init(string data)
    {
        if (ENCInstance.DevelopmentDebug) Debug.LogWarning("[N]未重写：初始化信息" + data);
    }
    public void PreRespawn(string data)
    {
        foreach (var i in Behaviors) i.NOMStart();
        Respawn(data);
    }
    protected virtual void Respawn(string data)
    {
        if (ENCInstance.DevelopmentDebug) Debug.LogWarning("[N]未重写：重新生成信息" + data);
    }
}
