using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于在服务器端也启动一个客户端<br></br>
/// 函数调用规则与ENCConnection一致
/// </summary>
public class ENCHost : ENCConnection
{
    internal CircularQueue<string> ReceivedData = new CircularQueue<string>();
    private ENCLocalClient _client;
    private bool _on;
    public override bool On()
    {
        return _on;
    }
    public static void Create(out ENCHost host,out ENCLocalClient client)
    {
        if (ENCInstance.ENCCorrespondent.ENCClient != null)
        {
            Debug.LogError("[E]客户端已经启动");
            host = null;
            client = null;
            return;
        }
        client=new ENCLocalClient();
        ENCInstance.ENCCorrespondent.ENCClient = client;
        host = new ENCHost(client);
        ENCInstance.ENCCorrespondent.ENCHost = host;
    }
    public ENCHost(ENCLocalClient client)
    {
        _client = client;
        ClientId = 0;
        ENCInstance.ENCCorrespondent.ENCClient.ClientId = ClientId;
        ENCInstance.LocalClientId = ClientId;
        _on = true;
        if(ENCInstance.DevelopmentDebug)Debug.Log("[E]本地连接已启动");
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
                ENCInstance.ServerRecvData?.Invoke(s, this);
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
        if (ENCInstance.DevelopmentDebug) Debug.Log("[E]本地连接(ENCHost)已关闭");
    }
}
/// <summary>
/// ENCLocalClient和ENCHost一起使用<br></br>
/// 在调用StartHost时由ENCHost创建
/// 函数调用规则与ENCClient一致
/// </summary>
public class ENCLocalClient : ENCClient
{
    internal CircularQueue<string> ReceivedData = new CircularQueue<string>();
    private bool _on=true;
    public override bool On()
    {
        return _on;
    }
    public ENCLocalClient() : base()//基类无参数的构造方法没有执行任何步骤
    {
        if(ENCInstance.DevelopmentDebug)Debug.Log("[E]本地客户端(ENCLocalClient)已启动");
    }
    public override void SendData(string data)
    {
        ENCInstance.ENCCorrespondent.ENCHost.ReceivedData.Write(data);
    }
    internal override void Update()
    {
        while (ReceivedData.Read(out var data))
        {
            try
            {
                ENCInstance.ClientRecvData?.Invoke(data);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
    public override void ShutDown()
    {
        ReceivedData = null;
        _on= false;
    }
}