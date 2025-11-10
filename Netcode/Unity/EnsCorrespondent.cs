using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

/// <summary>
/// SRC框架用于实现远程服务器的交互和房间管理<br></br>
/// 对于SRC层，加入时不会获取id或调用ENC事件(仅仅触发SRC事件)，而是加入房间时获取房间内id并触发ENC事件<br></br>
/// 连接成功和加入房间(分配id)之间只会发送请求信息<br></br>
/// ENC层的服务器相当于SRC层的房间<br></br>
/// 连接时，先触发SRC连接，加入房间后，先分配id(房间和客户端的，防bug)，再促发ENC连接和Id分配<br></br>
/// 断开时如果退出了房间则触发ENC断开，之后触发SRC断开
/// </summary>
public class EnsCorrespondent :MonoBehaviour
{
    [Header("ENC")]
    public string IP = "127.0.0.1";
    public int Port = 65432;
    public enum NetworkMode
    {
        None, Server, Client, Host
    }
    public NetworkMode networkMode;

    public ProtocolWrapper.ConcurrentType recvMode;

    public ProtocolWrapper.ProtocolType protocolType;
    [Space]

    public float KeyExistTime = 5f;//关键信息忽略时长
    public float KeySendInterval = 0.2f;//未确认的关键信息发送时长
    public float RKeyExistTime = 5f;//返回的关键信息忽略时长

    public bool MessyLog = false;
    public bool DevelopmentDebug = true;
    public bool ShowGeneralEvent = true;

    /// <summary>
    /// 上次接收心跳检测时间超过此阈值会认为断开了连接
    /// </summary>
    public float DisconnectThreshold = 3f;
    /// <summary>
    /// 发送心跳检测消息的间隔
    /// </summary>
    public float HeartbeatMsgInterval = 0.2f;

    [Space]
    public bool LogOnAllocateId = false;
    public bool LogOnAutoAssignedId = true;
    public bool LogOnManualAssignedId = true;


    public EnsServer Server;
    public EnsClient Client;
    public EnsHost Host;

    protected virtual void OnValidate()
    {
        if (DisconnectThreshold <= HeartbeatMsgInterval) DisconnectThreshold = HeartbeatMsgInterval + 0.1f;
    }

    private void Awake()
    {
        EnsInstance.Corr = this;

        EnsInstance.MessyLog = MessyLog;
        EnsInstance.DevelopmentDebug = DevelopmentDebug;
        EnsInstance.ShowGeneralEvent = ShowGeneralEvent;

        EnsInstance.KeyExistTime = KeyExistTime;
        EnsInstance.KeySendInterval = KeySendInterval;
        EnsInstance.RKeyExistTime = RKeyExistTime;

        EnsInstance.DisconnectThreshold = DisconnectThreshold;
        EnsInstance.HeartbeatMsgInterval = HeartbeatMsgInterval;

        ProtocolWrapper.Protocol.mode = recvMode;
        ProtocolWrapper.Protocol.type = protocolType;
        ProtocolWrapper.Protocol.DevelopmentDebug = DevelopmentDebug;

        EnsInstance.LogOnAllocateId = LogOnAllocateId;
        EnsInstance.LogOnAutoAssignedId = LogOnAutoAssignedId;
        EnsInstance.LogOnManualAssignedId = LogOnManualAssignedId;

        EnsEventRegister.RegistUnity();
    }
    protected void UpdateServerAndClient()//Clear send buffer and handle recv buffer
    {
        if (networkMode == NetworkMode.Host || networkMode == NetworkMode.Server)
        {
            Server.Update();
        }
        if (networkMode == NetworkMode.Host || networkMode == NetworkMode.Client)
        {
            if (Client != null)
            {
                Client.Update();
            }
            else Debug.LogWarning("[E]客户端初始化中");
        }
    }
    private void Update()
    {
        foreach (var p in EnsNetworkObjectManager.GetPriority().ToArray())//创建副本避免因修改产生错误
        {
            EnsNetworkObjectManager.Update(p);
            UpdateServerAndClient();
        }
        EnsEventRegister.LoopCommon();
        EnsEventRegister.LoopClient();
    }
    protected virtual void FixedUpdate()
    {
        foreach (var p in EnsNetworkObjectManager.GetFixedPriority().ToArray())//创建副本避免因修改产生错误
        {
            EnsNetworkObjectManager.FixedUpdate(p);
        }
    }
    public void StartHost()
    {
        if (networkMode != NetworkMode.None)
        {
            Debug.LogWarning("[N]已启动，关闭后才可调用");
            return;
        }
        if (!IPAddress.TryParse(IP, out _) || Port < 0 || Port > 65535)
        {
            Debug.Log("[N]输入的IP或端口有误");
            return;
        }

        networkMode = NetworkMode.Host;
        EnsHost.Create(out var host, out var client);
        Server = new EnsServer(Port);
        Server.ClientConnections.Add(host.ClientId,host);

        EnsInstance.OnServerConnect.Invoke(host.ClientId);
    }
    public void StartServer()
    {
        if (networkMode != NetworkMode.None)
        {
            Debug.LogWarning("[N]已启动，关闭后才可调用");
            return;
        }
        if (!IPAddress.TryParse(IP, out _) || Port < 0 || Port > 65535)
        {
            Debug.Log("[N]输入的IP或端口有误");
            return;
        }

        networkMode = NetworkMode.Server;
        Server = new EnsServer(Port);
    }
    public void StartClient()
    {
        if (networkMode != NetworkMode.None)
        {
            Debug.LogWarning("[E]已启动，关闭后才可调用");
            return;
        }
        if (!IPAddress.TryParse(IP, out _) || Port < 0 || Port > 65535)
        {
            Debug.Log("[E]输入的IP或端口有误");
            return;
        }

        try
        {
            EnsInstance.ClientConnectRejected = true;
            networkMode = NetworkMode.Client;
            Client = new EnsClient(IP, Port);
        }
        catch (Exception e)
        {
            Debug.LogError("[E]客户端启动失败，IP=" + IP + " Port=" + Port + " Log:" + e.ToString());
        }
    }
    /// <summary>
    /// >0加入 ==0创建 <0离开
    /// </summary>
    public void JoinRoom(int id)
    {
        if(networkMode != NetworkMode.Client)
        {
            Debug.Log("[S]未启动客户端模式");
            return;
        }
        if (id >= 0 && EnsInstance.PresentRoomId != 0)
        {
            Debug.LogError("[S]已加入房间，无法再加入");
            return;
        }
        if (id < 0 && EnsInstance.PresentRoomId == 0)
        {
            Debug.LogError("[S]未加入房间，无法离开");
            return;
        }
        Client.SendData("kR]"+id);
    }
    public virtual void ShutDown()
    {
        //关闭后访问器会返回null
        try
        {
            if (networkMode != NetworkMode.None&&EnsInstance.ClientConnectRejected)
            {
                EnsInstance.OnConnectionRejected?.Invoke();
                EnsInstance.ClientConnectRejected = false;
            }
            if (networkMode == NetworkMode.Server)
            {
                if (Server != null)
                {
                    Server.ShutDown();
                    Server.Dispose();
                    Server = null;
                }
            }
            else if (networkMode == NetworkMode.Client)
            {
                if (Client != null)
                {
                    Client.ShutDown();
                    Client.Dispose();
                    Client = null;
                }
            }
            else if (networkMode == NetworkMode.Host)
            {
                if (Server != null)//关闭Server->关闭Host->关闭LocalClient
                {
                    Server.ShutDown();
                    Server.Dispose();
                    Server = null;
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        networkMode = NetworkMode.None;
        if (!EnsInstance.RoomExitInvoke)
        {
            EnsInstance.OnExitRoom.Invoke();
        }
        if (!EnsInstance.ServerDisconnectInvoke)
        {
            EnsInstance.OnServerDisconnect?.Invoke();
        }
        EnsInstance.HasAuthority = false;
        EnsInstance.PresentRoomId = 0;
    }
    public void Request()
    {

    }

    private void OnApplicationQuit()
    {
        ShutDown();
    }
}
