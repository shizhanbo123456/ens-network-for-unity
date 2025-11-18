using UnityEngine;
/// <summary>
/// ENCLocalClient和ENCHost一起使用<br></br>
/// 在调用StartHost时由ENCHost创建
/// 函数调用规则与ENCClient一致
/// </summary>
internal class ENCLocalClient : EnsClient
{
    internal CircularQueue<string> ReceivedData = new CircularQueue<string>();
    private bool _on = true;
    internal override bool On()
    {
        return _on;
    }
    internal override bool Initilized()
    {
        return true;
    }
    public ENCLocalClient() : base()//基类无参数的构造方法没有执行任何步骤
    {
        if (EnsInstance.DevelopmentDebug) Debug.Log("[E]本地客户端(ENCLocalClient)已启动");
    }
    internal override void SendData(string data)
    {
        EnsInstance.Corr.Host.ReceivedData.Write(data);
    }
    internal override void Update()
    {
        while (ReceivedData.Read(out var data))
        {
            try
            {
                EnsInstance.ClientRecvData?.Invoke(data);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
    internal override void ShutDown()
    {
        ReceivedData = null;
        _on = false;
    }
}