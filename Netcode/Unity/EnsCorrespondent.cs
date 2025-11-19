using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class EnsCorrespondent :MonoBehaviour
{
    [Tooltip("The ip of the server which you gonna connect")]
    public string IP = "127.0.0.1";
    [Tooltip("The port of connections, for both server and clients")]
    public int Port = 65432;
    public enum NetworkMode
    {
        None, Server, Client, Host
    }
    [Tooltip("Which character the current game is at")]
    public NetworkMode networkMode;

    [Tooltip("In which way the client would receive data multithreading or asynchronous")]
    public ProtocolWrapper.ConcurrentType recvMode;
    [Tooltip("Which protocol would be used, make sure it uses the same protocol as the server")]
    public ProtocolWrapper.ProtocolType protocolType;

    [Space]
    [Tooltip("How long the local key will exist, it includes sending and ignoring response")]
    public float KeyExistTime = 5f;//关键信息忽略时长
    [Tooltip("The interval of unconfirmed keys to send")]
    public float KeySendInterval = 0.2f;//未确认的关键信息发送时长
    [Tooltip("How long will you ignore key messages after confirmed")]
    public float RKeyExistTime = 5f;//返回的关键信息忽略时长

    [Space]
    public bool DevelopmentDebug = true;

    [Space]
    /// <summary>
    /// 上次接收心跳检测时间超过此阈值会认为断开了连接
    /// </summary>
    [Tooltip("How long will a connection be reset since the last message received")]
    public float DisconnectThreshold = 3f;
    /// <summary>
    /// 发送心跳检测消息的间隔
    /// </summary>
    [Tooltip("The interval to send heartbeat message, it's also used to confirm the delay between client and server")]
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

        EnsInstance.DevelopmentDebug = DevelopmentDebug;

        EnsInstance.KeyExistTime = KeyExistTime;
        EnsInstance.KeySendInterval = KeySendInterval;
        EnsInstance.RKeyExistTime = RKeyExistTime;

        EnsInstance.DisconnectThreshold = DisconnectThreshold;
        EnsInstance.HeartbeatMsgInterval = HeartbeatMsgInterval;

        ProtocolWrapper.Protocol.mode = recvMode;
        ProtocolWrapper.Protocol.type = protocolType;
        ProtocolWrapper.Protocol.DevelopmentDebug = DevelopmentDebug;

        EnsClientEventRegister.RegistUnity();

        Loop.InitClient();
        Loop.InitCommon();
    }
    protected void UpdateServerAndClient()//Clear send buffer and handle recv buffer
    {
        if (networkMode == NetworkMode.Host || networkMode == NetworkMode.Server)
        {
            Server.Update();
        }
        if (networkMode == NetworkMode.Host || networkMode == NetworkMode.Client)
        {
            Client.Update();
        }
    }
    private void Update()
    {
        foreach (var p in EnsNetworkObjectManager.GetPriority().ToArray())//创建副本避免因修改产生错误
        {
            EnsNetworkObjectManager.Update(p);
            UpdateServerAndClient();
        }
        Loop.LoopCommon();
        Loop.LoopClient();
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
        Server = new EnsServer(IPAddress.Any,Port);
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
        Server = new EnsServer(IPAddress.Any,Port);
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
