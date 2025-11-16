using ProtocolWrapper;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using request = Ens.Request;
public class EnsEventRegister
{
    private static EnsCorrespondent Corr
    {
        get=>EnsInstance.Corr;
    }
    public static void RegistUnity()
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

        Client_C();
        Client_E();

        Client_A();
        Client_H();
        Client_D();
        Client_F();
        Client_S();
        Client_f();
        Client_Q();
        Client_Any();
        RegistClientRequests();

        ForceInvokeOnce_Server();
        ForceInvokeOnce_Room();
    }
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
    private static void RegistClientRequests()
    {
        EnsClientRequest.RegistRequest(new request.Client.CreateRoom());
        EnsClientRequest.RegistRequest(new request.Client.SetRule());
        EnsClientRequest.RegistRequest(new request.Client.JoinRoom());
        EnsClientRequest.RegistRequest(new request.Client.GetRule());
        EnsClientRequest.RegistRequest(new request.Client.ExitRoom());
        EnsClientRequest.RegistRequest(new request.Client.GetRoomList());
        EnsClientRequest.RegistRequest(new request.Client.SetInfo());
        EnsClientRequest.RegistRequest(new request.Client.GetInfo());
    }
    protected static void Server_Any()
    {
        EnsInstance.ServerRecvData += (data, conn) =>
        {
            if (data.Length > 2) conn.hbRecvTime.ReachAfter(EnsInstance.DisconnectThreshold);
        };
    }
    protected static void Client_Any()
    {
        EnsInstance.ClientRecvData += (data) =>
        {
            if (data.Length > 2)
            {
                EnsInstance.Corr.Client.hbRecvTime.ReachAfter(EnsInstance.DisconnectThreshold);
            }
        };
    }
    protected static void Client_C()
    {
        //成功连接
        EnsInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'C')
            {
                var i = int.Parse(data.Substring(3, data.Length - 3));
                EnsInstance.LocalClientId = i;
                EnsInstance.OnServerConnect.Invoke();
            }
        };
    }
    protected static void Client_E()
    {
        //事件
        EnsInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'E')
            {
                string[] s = data.Substring(3, data.Length - 3).Split('#');
                int e = int.Parse(s[0]);
                int i = int.Parse(s[1]);
                if (e == 1) EnsInstance.OnClientEnter?.Invoke(i);
                else if (e == 2) EnsInstance.OnClientExit?.Invoke(i);
                else Debug.LogError("[E]存在错误的事件消息 " + data);
            }
        };
    }
    protected static void Server_H()
    {
        EnsInstance.ServerRecvData += (data, conn) =>
        {
            if (data[1] == 'H')
            {
                int d = int.Parse(data.Substring(3, data.Length - 3));
                conn.delay = ((int)(Utils.Time.time * 1000) - d) / 2;
            }
        };
    }
    protected static void Client_H()
    {
        EnsInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'H')
            {
                EnsInstance.Corr.Client?.SendData(data);
            }
        };
    }
    protected static void Server_D()
    {
        EnsInstance.ServerRecvData += (data, conn) =>
        {
            if (data[1] == 'D')
            {
                conn.ShutDown();
            }
        };
    }
    protected static void Client_D()
    {
        EnsInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'D')
            {
                EnsInstance.Corr.ShutDown();
            }
        };
    }

    protected static void Server_A()
    {
        EnsInstance.ServerRecvData += (data, conn) =>
        {
            if (data[1] == 'A')
            {
                if (conn.room == null) return;
                int d = int.Parse(data.Substring(3, data.Length - 3));
                conn.room.SetAuthority(d);
            }
        };
    }
    protected static void Client_A()
    {
        EnsInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'A')
            {
                int d = int.Parse(data.Substring(3, data.Length - 3));
                EnsInstance.OnAuthorityChanged?.Invoke(d == 1);
            }
        };
    }
    protected static void Server_F()
    {
        EnsInstance.ServerRecvData += (data, connection) =>
        {
            if (data[1] == 'F')
            {
                if (connection.room == null) return;
                var s = Format.SplitWithBoundaries(data.Substring(3, data.Length - 3),'#');
                string target = s[2];
                if (target[0] == '-')
                {
                    if (target[1] == '1')//全部
                    {
                        connection.room.Broadcast(data);
                    }
                    else if (target[1] == '2')//忽略自身
                    {
                        connection.room.Broadcast(data, connection.ClientId);
                    }
                }
                else
                {
                    connection.room.PTP(data, Format.StringToList(target,int.Parse));
                }
            }
        };
    }
    protected static void Client_F()
    {
        EnsInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'F')
            {
                var s = Format.SplitWithBoundaries(data.Substring(3, data.Length - 3), '#');
                int id = int.Parse(s[0]);
                string func = s[1];
                EnsBehaviour obj = EnsNetworkObjectManager.GetObject(id);
                if (s.Count >= 4)
                {
                    string param = s[3];
                    obj.CallFuncLocal(func, param);
                }
                else
                {
                    obj.CallFuncLocal(func);
                }
            }
        };
    }
    protected static void Server_S()
    {
        EnsInstance.ServerRecvData += (data, connection) =>
        {
            if (data[1] == 'S')
            {
                if (connection.room == null) return;
                var s = Format.SplitWithBoundaries(data.Substring(3, data.Length - 3), '#');
                string target = s[2];
                if (target[0] == '-')
                {
                    if (target[1] == '1')//全部
                    {
                        foreach (var i in Corr.Server.ClientConnections.Values)
                        {
                            SyncSendTo(s, connection.delay, i);
                        }
                    }
                    else if (target[1] == '2')//忽略自身
                    {
                        foreach (var i in Corr.Server.ClientConnections.Values)
                        {
                            if (i.ClientId == connection.ClientId) continue;
                            SyncSendTo(s, connection.delay, i);
                        }
                    }
                }
                else
                {
                    var targets = Format.StringToList(target, int.Parse);
                    foreach (var i in Corr.Server.ClientConnections.Values)
                    {
                        if (targets.Contains(i.ClientId))
                            SyncSendTo(s, connection.delay, i);
                    }
                }
            }
        };
    }
    protected static void Client_S()
    {
        EnsInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'S')
            {
                var s = Format.SplitWithBoundaries(data.Substring(3, data.Length - 3), '#');
                int id = int.Parse(s[0]);
                string func = s[1];
                EnsBehaviour obj = EnsNetworkObjectManager.GetObject(id);
                if (obj == null)
                {
                    if (EnsInstance.DevelopmentDebug) Debug.LogWarning("[N]无物体id=" + id);
                    return;
                }
                obj.DelayInvoke(s);
            }
        };
    }
    protected static void Server_f()
    {
        //物体Id同步
        EnsInstance.ServerRecvData += (data, connection) =>
        {
            if (data[1] == 'f')
            {
                if (connection.room == null) return;
                var s = Format.SplitWithBoundaries(data.Substring(3, data.Length - 3), '#');
                string target = s[2];
                int behaviourCount = int.Parse(s[4]);
                data = data[0] + "f]" + s[0] + "#" + s[1] + "#" + s[2] + "#" + s[3] + "#" + connection.room.CreatedId;
                connection.room.CreatedId += behaviourCount;
                if (target[0] == '-')
                {
                    if (target[1] == '1')//全部
                    {
                        connection.room.Broadcast(data);
                    }
                    else if (target[1] == '2')//忽略自身
                    {
                        connection.room.Broadcast(data, connection.ClientId);
                    }
                }
                else
                {
                    connection.room.PTP(data, Format.StringToList(target, int.Parse));
                }
            }
        };
    }
    protected static void Client_f()
    {
        EnsInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'f')
            {
                var s = Format.SplitWithBoundaries(data.Substring(3, data.Length - 3), '#');
                int id = int.Parse(s[0]);
                //string func = s[1];
                EnsSpawner obj = EnsInstance.NOMSpawner;
                int idStart = int.Parse(s[s.Count - 1]);
                string param = s[3];
                obj.Create(param, idStart);
            }
        };
    }
    protected static void Server_Q()
    {
        EnsInstance.ServerRecvData += (data, conn) =>
        {
            if (data[1] == 'Q')
            {
                var s=Format.SplitWithBoundaries(data.Substring(3, data.Length - 3),'#');
                string reply=EnsServerRequest.OnRecvRequest(s[0], s[1], conn);
                if (reply == string.Empty) return;
                conn.SendData(data[0] + "Q]{" + s[0] + "}#{" + reply+"}");
            }
        };
    }
    protected static void Client_Q()
    {
        EnsInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'Q')
            {
                var s = Format.SplitWithBoundaries(data.Substring(3, data.Length - 3), '#');
                EnsClientRequest.RecvReply(s[0], s[1]);
            }
        };
    }

    protected static void ForceInvokeOnce_Server()
    {
        EnsInstance.OnServerConnect += () =>
        {
            EnsInstance.ServerDisconnectInvoke = false;
        };
        EnsInstance.OnServerDisconnect += () =>
        {
            EnsInstance.ServerDisconnectInvoke = true;
        };
    }
    protected static void ForceInvokeOnce_Room()
    {
        EnsInstance.OnCreateRoom += () =>
        {
            EnsInstance.RoomExitInvoke = false;
        };
        EnsInstance.OnJoinRoom += () =>
        {
            EnsInstance.RoomExitInvoke = false;
        };
        EnsInstance.OnExitRoom += () =>
        {
            EnsInstance.RoomExitInvoke = true;
        };
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


    public static void InitCommon()
    {
        Utils.Time.Init();
    }
    public static void InitClient()
    {

    }
    public static void InitServer()
    {

    }
    public static void LoopCommon()
    {
        Utils.Time.Update();
    }
    public static void LoopClient()
    {
        Broadcast.Update();
        EnsClientRequest.Update();
    }
    public static void LoopServer()
    {

    }
}