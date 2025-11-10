using System;

public class SRCInstance
{
    public static SRCCorrespondent SRCCorrespondent;

    public static Action OnServerConnected;
    public static Action OnServerDisconnect;
    public static Action OnJoinFailed;

    //0代表没有加入，和远程服务器断开连接后重置为0
    //可被设置为 0 或 >0
    //加入房间第一时刻被设置为房间号，离开房间后(事件触发后)和ShutDown最后一刻被设置为0
    public static int PresentRoomId = 0;

    //是否启动了远程联机模式
    //连接远程服务器前一刻被设置为true，ShutDown最后被设置为false
    public static bool SRCOn = false;
    //是否在SRC模式下作为Host身份(本质上还是client)
    //接收到加入房间消息的第一时刻被设置为true，离开房间后(事件触发后)和ShutDown被设置为false
    public static bool SRCHost = false;


    //确保断开连接事件只会触发一次(避免多次调用ShutDown)
    //用户无需在意此参数
    public static bool DisconnectInvoke = true;
}
