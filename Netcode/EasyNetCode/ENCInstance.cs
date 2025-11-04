using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class ENCInstance
{
    public static ENCCorrespondent ENCCorrespondent;

    //直接在处理C事件时调用(最优先)
    public static int LocalClientId=-1;

    public static bool MessyLog;
    public static bool DevelopmentDebug;
    public static bool ShowGeneralEvent;

    public static float DisconnectThreshold = 3f;// 上次接收心跳检测时间超过此阈值会认为断开了连接
    public static float HeartbeatMsgInterval = 0.2f;// 发送心跳检测消息的间隔

    public static float KeyExistTime = 3f;//关键信息忽略时长（对方）
    public static float KeySendInterval = 0.05f;//未确认的关键信息发送时长
    public static float RKeyExistTime = 5f;//返回的关键信息忽略时长（本地）



    public static Action<int> OnClientConnect;//有新用户连接到服务器时触发(新用户自身不调用)              ENCConnection连接后向其它连接发送
    public static Action<int> OnClientDisconnect;//有用户与服务器断开时调用(断开的用户自身不调用)         ENCHeartBeat的ServerSRHB广播

    public static Action<int> OnServerConnect;//自身连接到服务器且ID分配后时调用                          通讯器接收到分配的id后调用
    public static Action OnServerDisconnect;//自身与服务器断开时调用                                 ENCHeartBeat的ClientSRHB调用

    public static Action OnConnectFailed;//客户端尝试连接服务器超时后调用

    //确保断开连接事件只会触发一次(避免多次调用ShutDown)
    //用户不需要注意此参数
    internal static bool DisconnectInvoke = true;//开启时设置为false，关闭时设置为true，用于确保OnServerDisconnect事件仅仅会调用一次


    //固定的处理信息的事件
    /// <summary>
    /// 可以在启动前设置，实例化每个连接时自动添加
    /// </summary>
    public static Action<string,ENCConnection> ServerRecvData;
    /// <summary>
    /// 可以在启动前设置，实例化客户端时自动添加
    /// </summary>
    public static Action<string> ClientRecvData;

    internal static string GetContent()
    {
        return ((int)(Utils.Time.time * 1000)).ToString();
    }
    internal static void OnRecvResponse(string data, ENCConnection conn)
    {
        conn.delay = ((int)(Utils.Time.time * 1000) - int.Parse(data)) / 2;
    }
}



public abstract class SR:Disposable//具有信息收发功能
{
    internal ReachTime hbRecvTime=new ReachTime(ENCInstance.DisconnectThreshold,ReachTime.InitTimeFlagType.ReachAfter);
    internal ReachTime hbSendTime = new ReachTime(ENCInstance.HeartbeatMsgInterval, ReachTime.InitTimeFlagType.ReachAfter);

    public abstract bool On();

    public abstract void SendData(string data);
    //CircularQueue ReceiveData();
    internal abstract void Update();
    public abstract void ShutDown();

    protected override void ReleaseUnmanagedMenory()
    {
        hbRecvTime=null;
        hbSendTime=null;
    }
}