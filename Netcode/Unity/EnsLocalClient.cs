using UnityEngine;
/// <summary>
/// ENCLocalClient和ENCHost一起使用<br></br>
/// 在调用StartHost时由ENCHost创建
/// 函数调用规则与ENCClient一致
/// </summary>
internal class ENCLocalClient : EnsClient
{
    internal CircularQueue<string> ReceivedData = new CircularQueue<string>();
    public ENCLocalClient()
    {
        _on = true;
    }
    internal override void SendData(string data)
    {
        EnsInstance.Corr.Host.ReceivedData.Write(data);
    }
    internal override void Update()
    {
        while (ReceivedData.Read(out var data)&&_on)
        {
            try
            {
                MessageHandlerClient.Invoke(data);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
    internal override void ShutDown()
    {
        _on = false;
        ReceivedData = null;
    }
    protected override void ReleaseManagedMenory()
    {
        
    }
    protected override void ReleaseUnmanagedMenory()
    {
        ReceivedData = null;
    }
}