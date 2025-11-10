using ProtocolWrapper;

public class SRCConnection:ENCConnection
{
    public SRCRoom room;
    internal SRCConnection(ProtocolBase _base, int index)
    {
        KeyLibrary = new KeyLibrary();

        Connection = _base;
        ClientId = index;
        //不是在实例化时调用"新玩家加入"，而是在加入房间时
        SendData("KC]" + ClientId);
    }
    public override void ShutDown()
    {
        if (!On()) return;
        if (room!=null)room.Broadcast("KE]2#" + ClientId);//防止还没加入房间
        KeyLibrary.Clear();
        Connection.ShutDown();
    }
    protected override void ReleaseUnmanagedMenory()
    {
        room= null;
        base.ReleaseUnmanagedMenory();
    }
}
