using ProtocolWrapper;
using System.Collections.Generic;

public class ServerBase : Disposable
{
    internal Dictionary<int, EnsConnection> ClientConnections = new Dictionary<int, EnsConnection>();
    internal EnsRoomManager RoomManager;
    internal ListenerBase Listener;

    public bool On;

    internal virtual void OnRecvConnection(ProtocolBase conn, int index)
    {
        EnsConnection connection = new EnsConnection(conn, index);
        connection.OnShutDown += OnConnectionShutDown;
        ClientConnections.Add(index, connection);
    }
    internal void OnConnectionShutDown(EnsConnection conn)
    {
        if (conn.room != null) conn.room.Exit(conn);
    }
    public void StartListening()
    {
        if (!Listener.Listening) Listener.StartListening();
    }
    public void EndListening()
    {
        if (Listener.Listening) Listener.EndListening();
    }
    public virtual void Update()
    {
        for (int index = ClientConnections.Count - 1; index >= 0; index--)
        {
            var i = ClientConnections[index];
            if (i.hbRecvTime.Reached)
            {
                i.ShutDown();
                i.Dispose();
                ClientConnections.Remove(i.ClientId);
                continue;
            }
            if (i.hbSendTime.Reached)
            {
                i.SendData(Header.H + ((int)(Utils.Time.time * 1000)).ToString());
                i.hbSendTime.ReachAfter(EnsInstance.HeartbeatMsgInterval);
            }
            i.Update();
        }
    }
    public virtual void ShutDown()
    {
        if (!On) return;
        On = false;
        EndListening();
        Listener.ShutDown();
        foreach (var i in ClientConnections.Values) i.ShutDown();
    }
    protected override void ReleaseManagedMenory()
    {
        foreach (var i in ClientConnections.Values) i.Dispose();
        ClientConnections.Clear();
        RoomManager.Dispose();
        Listener.Dispose();
        foreach (var i in ClientConnections.Values) i.Dispose();
        ClientConnections.Clear();
        base.ReleaseManagedMenory();
    }
    protected override void ReleaseUnmanagedMenory()
    {
        ClientConnections = null;
        RoomManager = null;
        Listener = null;
        ClientConnections = null;
        base.ReleaseUnmanagedMenory();
    }
}