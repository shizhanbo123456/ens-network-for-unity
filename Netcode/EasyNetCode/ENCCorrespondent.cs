using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

/// <summary>
/// ENC框架用于简化客户端和服务器端的通信<br></br>
/// 场景中实例化ENCCorrespondent后，调用StartServer/StartClient/ShutDown来选择作为什么<br></br>
/// 启动前在ENCInstance中设置信息读取事件<br></br>
/// 启动后访问ENCServer的ENCConnection或Client即可收发消息
/// </summary>
public class ENCCorrespondent : MonoBehaviour
{
    [Header("ENC")]
    [SerializeField]private string ip = "127.0.0.1";
    public string IP
    {
        get { return ip; }
        set 
        { 
            ip = value; 
            if(ENCInstance.DevelopmentDebug)Debug.Log("[E]ENC IP设置为" + value); 
        }
    }
    public int Port=65432;
    public enum NetworkMode
    {
        None,Server,Client,Host
    }
    public NetworkMode networkMode;
    public enum Mode
    {
        Multithreading,Asynchronous
    }
    public Mode recvMode;

    public ProtocolWrapper.ProtocolType protocolType;
    [Space]

    public float KeyExistTime=5f;//关键信息忽略时长
    public float KeySendInterval=0.2f;//未确认的关键信息发送时长
    public float RKeyExistTime = 5f;//返回的关键信息忽略时长

    public bool MessyLog=false;
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



    protected ENCServer server;
    public ENCServer ENCServer 
    {
        get { if (server != null && server.On) return server; else return null; }
        set { server = value; }
    }//只可以启动一个
    protected ENCClient client;
    public ENCClient ENCClient 
    {
        get { if (client != null && client.On()) return client; else return null; }
        set { client = value; }
    }//
    protected ENCHost host;
    public ENCHost ENCHost
    {
        get { if (host != null && host.On()) return host; else return null; }
        set { host = value; }
    }

    protected virtual void OnValidate()
    {
        if (DisconnectThreshold <= HeartbeatMsgInterval) DisconnectThreshold = HeartbeatMsgInterval + 0.1f;
    }
    private void Awake()
    {
        ApplySettings();
        ApplyEvents();
    }
    protected virtual void ApplySettings()
    {
        ENCInstance.ENCCorrespondent = this;

        ENCInstance.MessyLog= MessyLog;
        ENCInstance.DevelopmentDebug = DevelopmentDebug;
        ENCInstance.ShowGeneralEvent = ShowGeneralEvent;

        ENCInstance.KeyExistTime = KeyExistTime;
        ENCInstance.KeySendInterval=KeySendInterval;
        ENCInstance.RKeyExistTime = RKeyExistTime;

        ENCInstance.DisconnectThreshold = DisconnectThreshold;
        ENCInstance.HeartbeatMsgInterval = HeartbeatMsgInterval;

        ProtocolWrapper.Protocol.mode = (ProtocolWrapper.Mode)recvMode;
        ProtocolWrapper.Protocol.type = protocolType;
        ProtocolWrapper.Protocol.DevelopmentDebug = DevelopmentDebug;
    }
    protected virtual void ApplyEvents()
    {
        new ENCEventRegister().RegistEvents();
    }

    protected bool DataCheck()
    {
        return IPAddress.TryParse(IP, out _) && Port >= 0 && Port <= 65535;
    }

    public virtual void StartServer()
    {
        if(networkMode != NetworkMode.None)
        {
            Debug.LogWarning("[E]已启动，关闭后才可调用");
            return;
        }
        if (!DataCheck())
        {
            Debug.Log("[E]输入的IP或端口有误");
            return;
        }

        Debug.Log("[E]启动了服务器端");
        networkMode = NetworkMode.Server;
        ENCServer = new ENCServer(Port);
    }
    public virtual void StartClient()
    {
        if (networkMode != NetworkMode.None)
        {
            Debug.LogWarning("[E]已启动，关闭后才可调用");
            return;
        }
        if (!DataCheck())
        {
            Debug.Log("[E]输入的IP或端口有误");
            return;
        }

        try
        {
            networkMode = NetworkMode.Client;
            ENCClient = new ENCClient(IP, Port);
            //ENCInstance.OnServerConnect?.Invoke();//-----------------------------------------------------[E]3
        }
        catch (Exception e)
        {
            Debug.LogError("[E]客户端启动失败，IP=" + IP + " Port=" + Port + " Log:" + e.ToString());
        }
    }
    public virtual void StartHost()
    {
        if (networkMode != NetworkMode.None)
        {
            Debug.LogWarning("[E]已启动，关闭后才可调用");
            return;
        }
        if (!DataCheck())
        {
            Debug.Log("[E]输入的IP或端口有误");
            return;
        }

        networkMode = NetworkMode.Host;
        ENCHost.Create(out var host, out var client);
        ENCServer = new ENCServer(Port);
        ENCServer.ClientConnections.Add(host);
        ENCInstance.OnServerConnect.Invoke(0);
    }
    public virtual void ShutDown()
    {
        //关闭后访问器会返回null
        try
        {
            if (networkMode == NetworkMode.Server)
            {
                if (server != null)
                {
                    server.Broadcast("[D]");
                    server.Update();
                    if (server.On) server.ShutDown();
                    server.Dispose();
                    server = null;
                }
            }
            else if (networkMode == NetworkMode.Client)
            {
                if (client != null)
                {
                    client.SendData("[D]");
                    client.Update();
                    if (client.On()) client.ShutDown();
                    client.Dispose();
                    client = null;
                }
            }
            else if (networkMode == NetworkMode.Host)
            {
                if (server != null)//关闭Server->关闭Host->关闭LocalClient
                {
                    server.Broadcast("[D]");
                    server.Update();
                    if (server.On) server.ShutDown();
                    server.Dispose();
                    server = null;
                }
            }
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
        networkMode = NetworkMode.None;
        if (!ENCInstance.DisconnectInvoke)
        {
            ENCInstance.OnServerDisconnect?.Invoke();
            Debug.Log("[E]ENC通讯器已关闭");
        }
    }

    protected virtual void Update()
    {
        UpdateServerAndClient();

        ProtocolWrapper.Protocol.Update();
    }
    protected void UpdateServerAndClient()//Clear send buffer and handle recv buffer
    {
        if (networkMode == NetworkMode.Host || networkMode == NetworkMode.Server)
        {
            ENCServer.Update();
        }
        if (networkMode == NetworkMode.Host || networkMode == NetworkMode.Client)
        {
            if (client!=null)
            {
                ENCClient.Update();
            }
            else Debug.LogWarning("[E]客户端初始化中");
        }
    }
    private void OnApplicationQuit()
    {
        ShutDown();
    }
}
