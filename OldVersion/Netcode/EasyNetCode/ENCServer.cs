using ProtocolWrapper;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

/// <summary>
/// 实例化时启动服务器端
/// </summary>
public class ENCServer:Disposable
{
    public static ENCServer encInstance;

    public List<ENCConnection> ClientConnections = new List<ENCConnection>();

    internal ListenerBase Listener;

    public bool On;
    public bool Listening()
    {
        if (Listener == null) return false;
        return Listener.Listening;
    }

    internal ENCServer(int port)
    {
        encInstance= this;
        Listener = Protocol.GetListener(IPAddress.Any, port);
        On = true;
    }
    internal ENCServer(IPAddress ip, int port)
    {
        encInstance = this;
        Listener = Protocol.GetListener(ip,port);
        On = true;
    }


    public void StartListening()
    {
        if(!Listener.Listening)Listener.StartListening();
    }
    public void EndListening()
    {
        if (Listener.Listening) Listener.EndListening();
    }

    internal virtual void OnRecvConnection(ProtocolBase conn,int index)
    {
        ENCConnection connection = new ENCConnection(conn, index);
        ClientConnections.Add(connection);
    }


    public virtual void Broadcast(string data)
    {
        foreach(var i in ClientConnections)i.SendData(data);
    }
    public virtual void Broadcast(string data,int self)
    {
        foreach (var i in ClientConnections) if(i.ClientId!=self)i.SendData(data);
    }
    public virtual void PTP(string data,int id)
    {
        foreach(var i in ClientConnections)if(id==i.ClientId)i.SendData(data);
    }
    public virtual void PTP(string data, List<int> id)
    {
        foreach (var i in ClientConnections) if (id.Contains(i.ClientId)) i.SendData(data);
    }









    internal virtual void Update()
    {
        for (int index = ClientConnections.Count - 1; index >= 0; index--)
        {
            var i = ClientConnections[index];
            if (!i.On())
            {
                i.Dispose();
                ClientConnections.RemoveAt(index);
                continue;
            }
            if (i.hbRecvTime.Reached)
            {
                i.ShutDown();
                i.Dispose();
                ClientConnections.RemoveAt(index);
                continue;
            }
            if (i.hbSendTime.Reached)
            {
                i.SendData("[H]" + ENCInstance.GetContent());
                i.hbSendTime.ReachAfter(ENCInstance.HeartbeatMsgInterval);
            }
            i.Update();
        }
    }


    /// <summary>
    /// 关闭服务器
    /// </summary>
    public virtual void ShutDown()
    {
        EndListening();
        On = false;
        Listener.ShutDown();
        foreach (var i in ClientConnections) i.ShutDown();
        encInstance = null;
        if (ENCInstance.ShowGeneralEvent) Debug.Log("[E]服务器端已关闭");
    }
    protected override void ReleaseManagedMenory()
    {
        Listener.Dispose();
        foreach (var i in ClientConnections) i?.Dispose();
        ClientConnections.Clear();
        base.ReleaseManagedMenory();
    }
    protected override void ReleaseUnmanagedMenory()
    {
        Listener = null;
        ClientConnections = null;
        encInstance = null;
        base.ReleaseUnmanagedMenory();
    }
}