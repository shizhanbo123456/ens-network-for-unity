using ProtocolWrapper;
using System;
using UnityEngine;

/// <summary>
/// 服务器使用，用于简化和客户端的通信
/// </summary>
public class EnsConnection:SR
{
    internal int ClientId;
    private KeyLibrary KeyLibrary;
    private ProtocolBase Connection;
    internal EnsRoom room;

    internal Action<EnsConnection> OnShutDown;

    internal int delay = 20;//20ms


    internal override bool On()
    {
        if (Connection == null) return false;
        return Connection.On;
    }

    protected EnsConnection() { }
    internal EnsConnection(ProtocolBase _base,int index)
    {
        KeyLibrary = new KeyLibrary();

        Connection = _base;
        ClientId = index;

        SendData(Header.kC + ClientId);
    }
    internal override void SendData(string data)
    {
        if (data[0] == 'k' || data[0]=='K') KeyLibrary.Add(data);
        else Connection.SendData(data);
    }
    internal override void Update()
    {
        var d = KeyLibrary.Update();
        foreach (var s in d) Connection.SendData(s);
        Connection.RefreshSendBuffer();
        var q=Connection.RefreshRecvBuffer();
        if (q == null) return;
        while (q.Read(out var data))
        {
            try
            {
                if (data[1] == 'K' || data[1]=='k')
                {
                    KeyLibrary.OnRecvData(data, out var skip, out data);
                    if (skip) continue;
                }
                EnsInstance.ServerRecvData?.Invoke(data, this);
            }
            catch
            {
            }
        }
    }
    internal override void ShutDown()
    {
        if (Connection==null||Connection.Cancelled) return;
        OnShutDown?.Invoke(this);
        Connection.SendData(Header.D);
        Connection.RefreshSendBuffer();

        KeyLibrary.Clear();
        Connection.ShutDown();

        Dispose();
    }
    protected override void ReleaseManagedMenory()
    {
        Connection?.Dispose();
        base.ReleaseManagedMenory();
    }
    protected override void ReleaseUnmanagedMenory()
    {
        Connection = null;
        KeyLibrary = null;
        room = null;
        base.ReleaseUnmanagedMenory();
    }
}
