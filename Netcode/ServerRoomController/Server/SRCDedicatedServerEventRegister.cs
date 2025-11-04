using ProtocolWrapper;
using UnityEngine;

public class SRCDedicatedServerEventRegister
{
    public void RegistEvent()
    {
        Server_Any();
        Client_Any();
        Server_R();
        S_RecvConnection();
        InfoTeleport();
    }
    protected virtual void Server_Any()
    {
        ENCInstance.ServerRecvData += (data, conn) =>
        {
            if (data.Length > 2) conn.hbRecvTime.ReachAfter(ENCInstance.DisconnectThreshold);
        };
    }
    protected virtual void Client_Any()
    {
        ENCInstance.ClientRecvData += (data) =>
        {
            if (data.Length > 2)
            {
                ENCInstance.ENCCorrespondent.ENCClient.hbRecvTime.ReachAfter(ENCInstance.DisconnectThreshold);
            }
        };
    }
    protected virtual void Server_R()
    {
        ENCInstance.ServerRecvData += (data, connection) =>
        {
            if (data[1] == 'R')
            {
                SRCConnection conn = connection as SRCConnection;
                if (conn == null)
                {
                    Debug.Log("连接的类型错误");
                    return;
                }
                //0创建 >0加入 <0离开
                int id = int.Parse(data.Substring(3, data.Length - 3));
                int r = 0;
                int clientId;
                if (id == 0) r = SRCDedicatedServer.Instance.RoomManager.CreateRoom(conn, out clientId);
                else if (id > 0) r = SRCDedicatedServer.Instance.RoomManager.JoinRoom(conn, id, out clientId);
                else r = SRCDedicatedServer.Instance.RoomManager.ExitRoom(conn, out clientId);

                conn.SendData("kR]" + r + "#" + clientId);
                if (SRCProgram.Instance.PrintRoomData) Debug.Log(SRCDedicatedServer.Instance.RoomManager.PrintData());
            }
        };
    }
    protected virtual void S_RecvConnection()
    {
        Protocol.OnRecvConnection += (conn, index) =>
        {
            SRCDedicatedServer.Instance.OnRecvConnection(conn, index);
        };
    }
    protected void InfoTeleport()
    {
        ENCInstance.ServerRecvData += (data, conn) =>
        {
            SRCConnection connection = conn as SRCConnection;
            if (connection.room == null)
            {
                Debug.Log("连接 " + connection.ClientId + " 未加入房间");
                return;
            }
            connection.room.HandleData(data, connection.ClientId);
        };
    }
}