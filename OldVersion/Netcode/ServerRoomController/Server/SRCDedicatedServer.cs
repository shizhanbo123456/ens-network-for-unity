using ProtocolWrapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class SRCDedicatedServer : ENCServer
{
    public static SRCDedicatedServer Instance;

    public SRCRoomManager RoomManager;

    public SRCDedicatedServer(IPAddress ip,int port) : base(ip,port)
    {
        new SRCDedicatedServerEventRegister().RegistEvent();

        Instance = this;
        RoomManager = new SRCRoomManager();
    }


    internal override void OnRecvConnection(ProtocolBase conn, int index)
    {
        SRCConnection connection = new SRCConnection(conn, index);
        ClientConnections.Add(connection);
    }



    public override void Broadcast(string data)
    {
        foreach (var i in ClientConnections) i.SendData(data);
    }
    public override void Broadcast(string data, int self)
    {
        foreach (var i in ClientConnections) if (i.ClientId != self) i.SendData(data);
    }
    public override void PTP(string data, int id)
    {
        foreach (var i in ClientConnections) if (id == i.ClientId) i.SendData(data);
    }
    public override void PTP(string data, List<int> id)
    {
        foreach (var i in ClientConnections) if (id.Contains(i.ClientId)) i.SendData(data);
    }



    public override void ShutDown()
    {
        base.ShutDown();
        Instance = null;
    }
    protected override void ReleaseManagedMenory()
    {
        foreach (var i in ClientConnections) i.Dispose();
        ClientConnections.Clear();
        RoomManager.Dispose();
        base.ReleaseManagedMenory();
    }
    protected override void ReleaseUnmanagedMenory()
    {
        ClientConnections = null;
        RoomManager = null;
        base.ReleaseUnmanagedMenory();
    }
}
