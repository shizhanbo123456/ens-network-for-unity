using ProtocolWrapper;
using System;
using UnityEngine;

/// <summary>
/// 服务器使用，用于简化和客户端的通信
/// </summary>
public class ENCConnection:SR
{
    public int ClientId;
    protected KeyLibrary KeyLibrary;
    internal ProtocolBase Connection;

    public int delay = 20;//20ms


    public bool Initialized
    {
        get
        {
            return Connection.Initialized;
        }
    }
    public bool Cancelled
    {
        get
        {
            return Connection.Cancelled;
        }
    }
    public override bool On()
    {
        if (Connection == null) return false;
        return Connection.On;
    }

    //SRCConnection会调用
    internal ENCConnection() { }
    internal ENCConnection(ProtocolBase _base,int index)
    {
        KeyLibrary = new KeyLibrary();

        Connection = _base;
        ClientId = index;

        ENCInstance.ENCCorrespondent.ENCServer.Broadcast("KE]1#"+ClientId,ClientId);
        SendData("KC]"+ClientId);
    }
    public override void SendData(string data)
    {
        //Debug.Log("[E]Connection发送" + data);
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
                ENCInstance.ServerRecvData?.Invoke(data, this);
            }
            catch(Exception e)
            {
                if(ENCInstance.MessyLog)Debug.LogError("[E]读取接收信息时发生异常" + data+" "+data.Length+" "+e.ToString());
            }
        }
    }
    public override void ShutDown()
    {
        if (!On()) return;
        if(ENCServer.encInstance!=null)ENCServer.encInstance.Broadcast("KE]2#" + ClientId);
        KeyLibrary.Clear();
        Connection.ShutDown();
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
        base.ReleaseUnmanagedMenory();
    }
}
