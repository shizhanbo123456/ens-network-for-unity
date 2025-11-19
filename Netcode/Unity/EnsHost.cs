using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于在服务器端也启动一个客户端<br></br>
/// 函数调用规则与ENCConnection一致
/// </summary>
internal class EnsHost : EnsConnection
{
    internal CircularQueue<string> ReceivedData = new CircularQueue<string>();
    private ENCLocalClient _client;

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
    internal override void SendData(string data)
    {
        if(_client!=null)_client.ReceivedData.Write(data);
    }
    internal override void Update()
    {
        while(ReceivedData.Read(out var s)&&_on)
        {
            try
            {
                MessageHandlerServer.Invoke(s, this);
            }
            catch(System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
    internal override void ShutDown()
    {
        _client.ShutDown();
        _on = false;
    }
}