using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于在服务器端也启动一个客户端<br></br>
/// 函数调用规则与ENCConnection一致
/// </summary>
public class EnsHost : EnsConnection
{
    internal CircularQueue<string> ReceivedData = new CircularQueue<string>();
    private ENCLocalClient _client;
    private bool _on;
    public override bool On()
    {
        return _on;
    }
    internal static void Create(out EnsHost host,out ENCLocalClient client)
    {
        if (EnsInstance.Corr.Client != null)
        {
            Debug.LogError("[E]客户端已经启动");
            host = null;
            client = null;
            return;
        }
        client=new ENCLocalClient();
        EnsInstance.Corr.Client = client;
        host = new EnsHost(client);
        EnsInstance.Corr.Host = host;
    }
    internal EnsHost(ENCLocalClient client)
    {
        _client = client;
        ClientId = 0;
        EnsInstance.LocalClientId = ClientId;
        _on = true;
    }
    public override void SendData(string data)
    {
        _client.ReceivedData.Write(data);
    }
    internal override void Update()
    {
        while(ReceivedData.Read(out var s))
        {
            try
            {
                EnsInstance.ServerRecvData?.Invoke(s, this);
            }
            catch(System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
    public override void ShutDown()
    {
        _client.ShutDown();
        _on = false;
        if (EnsInstance.DevelopmentDebug) Debug.Log("[E]本地连接(ENCHost)已关闭");
    }
}