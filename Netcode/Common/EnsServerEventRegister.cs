using System.Collections.Generic;
using System.Text;
using request = Ens.Request;
public class EnsServerEventRegister
{
    public static void RegistDedicateServer()
    {
        Server_A();
        Server_H();
        Server_D();
        Server_F();
        Server_S();
        Server_f();
        Server_Q();
        Server_Any();
        RegistServerRequests();
    }
    private static void RegistServerRequests()
    {
        EnsServerRequest.RegistRequest(new request.Server.CreateRoom());
        EnsServerRequest.RegistRequest(new request.Server.SetRule());
        EnsServerRequest.RegistRequest(new request.Server.JoinRoom());
        EnsServerRequest.RegistRequest(new request.Server.GetRule());
        EnsServerRequest.RegistRequest(new request.Server.ExitRoom());
        EnsServerRequest.RegistRequest(new request.Server.GetRoomList());
        EnsServerRequest.RegistRequest(new request.Server.SetInfo());
        EnsServerRequest.RegistRequest(new request.Server.GetInfo());
    }
    protected static void Server_Any()
    {
        MessageHandlerServer.RegistAny((data, conn) =>
        {
            if (data.Length > 2) conn.hbRecvTime.ReachAfter(EnsInstance.DisconnectThreshold);
        });
    }
    protected static void Server_H()
    {
        MessageHandlerServer.Regist('H',(data, conn) =>
        {
            int d = int.Parse(data.Substring(3, data.Length - 3));
            conn.delay = ((int)(Utils.Time.time * 1000) - d) / 2;
        });
    }
    protected static void Server_D()
    {
        MessageHandlerServer.Regist('D',(data, conn) =>
        {
            conn.ShutDown();
        });
    }

    protected static void Server_A()
    {
        MessageHandlerServer.Regist('A',(data, conn) =>
        {
            if (conn.room == null) return;
            int d = int.Parse(data.Substring(3, data.Length - 3));
            conn.room.SetAuthority(d);
        });
    }
    protected static void Server_F()
    {
        MessageHandlerServer.Regist('F',(data, conn) =>
        {
            if (conn.room == null) return;
            var s = Format.SplitWithBoundaries(data.Substring(3, data.Length - 3), '#');
            string target = s[2];
            if (target[0] == '-')
            {
                if (target[1] == '1')//全部
                {
                    conn.room.Broadcast(data);
                }
                else if (target[1] == '2')//忽略自身
                {
                    conn.room.Broadcast(data, conn.ClientId);
                }
                else if (target[1] == '3')//权限所在
                {
                    conn.room.PTP(data, conn.room.CurrentAuthorityAt);
                }
            }
            else
            {
                conn.room.PTP(data, Format.StringToList(target, int.Parse));
            }
        });
    }
    protected static void Server_S()
    {
        MessageHandlerServer.Regist('S',(data, conn) =>
        {
            if (conn.room == null) return;
            var s = Format.SplitWithBoundaries(data.Substring(3, data.Length - 3), '#');
            string target = s[2];
            if (target[0] == '-')
            {
                if (target[1] == '1')//全部
                {
                    foreach (var i in EnsServer.Instance.ClientConnections.Values)
                    {
                        SyncSendTo(s, conn.delay, i);
                    }
                }
                else if (target[1] == '2')//忽略自身
                {
                    foreach (var i in EnsServer.Instance.ClientConnections.Values)
                    {
                        if (i.ClientId == conn.ClientId) continue;
                        SyncSendTo(s, conn.delay, i);
                    }
                }
            }
            else
            {
                var targets = Format.StringToList(target, int.Parse);
                foreach (var i in EnsServer.Instance.ClientConnections.Values)
                {
                    if (targets.Contains(i.ClientId))
                        SyncSendTo(s, conn.delay, i);
                }
            }
        });
    }
    protected static void Server_f()
    {
        //物体Id同步
        MessageHandlerServer.Regist('f',(data, conn) =>
        {
            if (conn.room == null) return;
            var s = Format.SplitWithBoundaries(data.Substring(3, data.Length - 3), '#');
            string target = s[2];
            int behaviourCount = int.Parse(s[4]);
            data = data[0] + "f]" + s[0] + "#" + s[1] + "#" + s[2] + "#" + s[3] + "#" + conn.room.CreatedId;
            conn.room.CreatedId += behaviourCount;
            if (target[0] == '-')
            {
                if (target[1] == '1')//全部
                {
                    conn.room.Broadcast(data);
                }
                else if (target[1] == '2')//忽略自身
                {
                    conn.room.Broadcast(data, conn.ClientId);
                }
            }
            else
            {
                conn.room.PTP(data, Format.StringToList(target, int.Parse));
            }
        });
    }
    protected static void Server_Q()
    {
        MessageHandlerServer.Regist('Q',(data, conn) =>
        {
            var s = Format.SplitWithBoundaries(data.Substring(3, data.Length - 3), '#');
            string reply = EnsServerRequest.OnRecvRequest(s[0], s[1], conn);
            if (reply == string.Empty) return;
            conn.SendData(data[0] + "Q]{" + s[0] + "}#{" + reply + "}");
        });
    }
    private static void SyncSendTo(List<string> s, int senderDelay, EnsConnection target)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("IF]");
        for (int i = 0; i < s.Count; i++)
        {
            if (i != 0) sb.Append('#');
            if (i != 3) sb.Append(s[i]);
            else sb.Append((int.Parse(s[3]) - senderDelay - target.delay).ToString());
        }
        target.SendData(sb.ToString());
    }
}