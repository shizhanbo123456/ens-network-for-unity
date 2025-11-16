using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class EnsCorrespondent :MonoBehaviour
{
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


    internal EnsServer Server;
    internal EnsClient Client;
    internal EnsHost Host;

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

        EnsEventRegister.RegistUnity();

        EnsEventRegister.InitClient();
        EnsEventRegister.InitCommon();
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
        EnsInstance.OnServerConnect.Invoke();
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
    public void SetServerListening(bool listening)
    {
        if (Server == null)
        {
            Debug.LogError("未启动服务器");
            return;
        }
        if(listening)Server.StartListening();
        else Server.EndListening();
    }

    public virtual void ShutDown()
    {
        try
        {
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
        if (EnsInstance.ClientConnectRejected)
        {
            EnsInstance.OnConnectionRejected?.Invoke();
            EnsInstance.ClientConnectRejected = false;
        }
        if (!EnsInstance.RoomExitInvoke)
        {
            EnsInstance.OnExitRoom.Invoke();
        }
        if (!EnsInstance.ServerDisconnectInvoke)
        {
            EnsInstance.LocalClientId = -1;
            EnsInstance.OnServerDisconnect?.Invoke();
        }
        EnsInstance.HasAuthority = false;
        EnsInstance.PresentRoomId = 0;
    }

    private void OnApplicationQuit()
    {
        ShutDown();
    }
}
