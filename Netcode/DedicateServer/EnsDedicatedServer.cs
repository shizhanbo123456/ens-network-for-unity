using ProtocolWrapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class EnsDedicatedServer:ServerBase
{
    public EnsDedicatedServer(IPAddress ip,int port)
    {
        Listener = Protocol.GetListener(ip, port);
        On = true;

        EnsEventRegister.RegistDedicateServer();

        RoomManager = new EnsRoomManager();

        Protocol.OnRecvConnection += (conn, index) => OnRecvConnection(conn, index);
    }



    public override void ShutDown()
    {
        if (!On) return;
        base.ShutDown();
        if (EnsInstance.ShowGeneralEvent) Debug.Log("[E]服务器端已关闭");
    }
}
