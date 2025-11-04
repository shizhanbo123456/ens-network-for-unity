using ProtocolWrapper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// NOM框架实现拥有相同编号的NOMBehavior脚本能够在客户端被调用时所有目标客户端都可以被同时调用<br></br>
/// NOMBehavior提供了广播函数的功能<br></br>
/// 分配Id时，场景中已经存在的分配为<0，过程中创建的自动分配>0Id<br></br>
/// 需要创建的NOMBehavior物体应当作为预制体放入NOMCorrespondent的Prefab中<br></br>
/// 利用NOMSpawner生成物体，可利用相关函数在其被创建时立即传入参数
/// </summary>
[RequireComponent(typeof(NOMSpawner))]
public class NOMCorrespondent : ENCCorrespondent
{
    [Space]
    [Header("NOM")]
    public bool LogOnAllocateId = false;
    public bool LogOnAutoAssignedId = true;
    public bool LogOnManualAssignedId=true;

    protected override void ApplySettings()
    {
        NOMInstance.NOMCorrespondent = this;

        NOMInstance.LogOnAllocateId = LogOnAllocateId;
        NOMInstance.LogOnAutoAssignedId= LogOnAutoAssignedId;
        NOMInstance.LogOnManualAssignedId = LogOnManualAssignedId;
        base.ApplySettings();
    }
    protected override void ApplyEvents()
    {
        new NOMEventRegister().RegistEvents();
    }
    protected override void Update()
    {
        foreach (var p in NOMInstance.GetPriority().ToArray())//创建副本避免因修改产生错误
        {
            NOMInstance.Update(p);
            UpdateServerAndClient();
        }
        Protocol.Update();
    }
    protected virtual void FixedUpdate()
    {
        foreach (var p in NOMInstance.GetFixedPriority().ToArray())//创建副本避免因修改产生错误
        {
            NOMInstance.FixedUpdate(p);
        }
    }
    public override void StartServer()
    {
        if (networkMode != NetworkMode.None)
        {
            Debug.LogWarning("[N]已启动，关闭后才可调用");
            return;
        }
        if (!DataCheck())
        {
            Debug.Log("[N]输入的IP或端口有误");
            return;
        }

        Debug.Log("[N]启动了服务器端");
        networkMode = NetworkMode.Server;
        ENCServer = new NOMServer(Port);
    }
    public override void StartHost()
    {
        if (networkMode != NetworkMode.None)
        {
            Debug.LogWarning("[N]已启动，关闭后才可调用");
            return;
        }
        if (!DataCheck())
        {
            Debug.Log("[N]输入的IP或端口有误");
            return;
        }

        networkMode = NetworkMode.Host;
        ENCHost.Create(out var host, out var client);
        ENCServer = new NOMServer(Port);
        ENCServer.ClientConnections.Add(host);
        ENCInstance.OnServerConnect.Invoke(0);
    }
}
