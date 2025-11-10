using Utils;

public abstract class SR : Disposable//具有信息收发功能
{
    internal ReachTime hbRecvTime = new ReachTime(EnsInstance.DisconnectThreshold, ReachTime.InitTimeFlagType.ReachAfter);
    internal ReachTime hbSendTime = new ReachTime(EnsInstance.HeartbeatMsgInterval, ReachTime.InitTimeFlagType.ReachAfter);

    public abstract bool On();

    public abstract void SendData(string data);
    //CircularQueue ReceiveData();
    internal abstract void Update();
    public abstract void ShutDown();

    protected override void ReleaseUnmanagedMenory()
    {
        hbRecvTime = null;
        hbSendTime = null;
    }
}