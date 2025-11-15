using ProtocolWrapper;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class EnsServer : ServerBase
{
    public static EnsServer Instance;


    internal EnsServer(int port)
    {
        Instance= this;
        Listener = Protocol.GetListener(IPAddress.Any, port);
        RoomManager = new EnsRoomManager(true);
        On = true;

        Protocol.OnRecvConnection = (conn, index) => OnRecvConnection(conn, index);
    }



    public override void ShutDown()
    {
        if (!On) return;
        base.ShutDown();
        Instance = null;
        if (EnsInstance.ShowGeneralEvent) Debug.Log("[E]服务器端已关闭");
    }
    protected override void ReleaseUnmanagedMenory()
    {
        Instance = null;
        base.ReleaseUnmanagedMenory();
    }
}
