using ProtocolWrapper;

/// <summary>
/// 实例化时启动客户端
/// </summary>
internal class EnsClient:SR
{
    protected KeyLibrary KeyLibrary;

    private ProtocolBase Client;

    protected bool _on;

    protected EnsClient(){ }
    internal EnsClient(string ip,int port)
    {
        KeyLibrary = new KeyLibrary();

        Client = Protocol.GetClient(ip,port);

        _on= true;
    }
    internal override void SendData(string data)
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
            EnsInstance.Corr.ShutDown();
            return;
        }
        if (!Client.Initialized)
        {
            Utils.Debug.LogWarning("客户端初始化中");
            return;
        }
        var d = KeyLibrary.Update();
        foreach (var s in d) Client.SendData(s);
        Client.RefreshSendBuffer();
        var q = Client.RefreshRecvBuffer();
        if (q == null) return;
        while(q.Read(out var data)&&_on)
        {
            try
            {
                if (data[1] == 'K' || data[1] == 'k')
                {
                    KeyLibrary.OnRecvData(data, out var skip, out data);
                    if (skip) continue;
                }
                MessageHandlerClient.Invoke(data);
            }
            catch
            {
            }
        }
    }
    internal override void ShutDown()
    {
        if (Client == null || Client.Cancelled) return;
        Client.SendData(Header.D);
        Client.RefreshSendBuffer();

        _on = false;
        Client.ShutDown();
        KeyLibrary.Clear();
    }

    protected override void ReleaseManagedMenory()
    {
        Client?.Dispose();
        base.ReleaseManagedMenory();
    }
    protected override void ReleaseUnmanagedMenory()
    {
        Client = null;
        KeyLibrary= null;
        base.ReleaseUnmanagedMenory();
    }
}
