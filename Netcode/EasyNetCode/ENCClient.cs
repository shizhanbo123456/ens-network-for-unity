using ProtocolWrapper;
using System;
using UnityEngine;

/// <summary>
/// 实例化时启动客户端
/// </summary>
public class ENCClient:SR
{
    public int ClientId;
    protected KeyLibrary KeyLibrary;

    private ProtocolBase Client;

    public override bool On()
    {
        if(Client == null)return false;
        return Client.On;
    }

    protected ENCClient(){ }
    public ENCClient(string ip,int port)
    {
        KeyLibrary = new KeyLibrary();

        Client = Protocol.GetClient(ip,port);
    }
    public override void SendData(string data)
    {
        if (data[0] == 'k' || data[0]=='K')
        {
            KeyLibrary.Add(data);
        }
        else Client.SendData(data);
    }
    internal override void Update()
    {
        if (hbRecvTime.Reached)
        {
            ShutDown();
            return;
        }
        var d = KeyLibrary.Update();
        foreach (var s in d) Client.SendData(s);
        Client.RefreshSendBuffer();
        var q = Client.RefreshRecvBuffer();
        if (q == null) return;
        while(q.Read(out var data))
        {
            try
            {
                if (data[1] == 'K' || data[1] == 'k')
                {
                    KeyLibrary.OnRecvData(data, out var skip, out data);
                    if (skip) continue;
                }
                ENCInstance.ClientRecvData?.Invoke(data);
            }
            catch (Exception e)
            {
                if(ENCInstance.MessyLog)Debug.LogError("[E]读取接收信息时发生异常" + data + " " + e.ToString());
            }
        }
    }
    public override void ShutDown()
    {
        Client.ShutDown();
        KeyLibrary.Clear();
    }

    protected override void ReleaseManagedMenory()
    {
        Client.Dispose();
        base.ReleaseManagedMenory();
    }
    protected override void ReleaseUnmanagedMenory()
    {
        Client = null;
        KeyLibrary= null;
        base.ReleaseUnmanagedMenory();
    }
}
