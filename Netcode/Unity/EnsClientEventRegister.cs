using Utils;
using request = Ens.Request;
public class EnsClientEventRegister
{
    public static void RegistUnity()
    {
        EnsServerEventRegister.RegistDedicateServer();

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
    protected static void Client_Any()
    {
        MessageHandlerClient.RegistAny((data) =>
        {
            if (data.Length > 2)
            {
                EnsInstance.Corr.Client.hbRecvTime.ReachAfter(EnsInstance.DisconnectThreshold);
            }
        });
    }
    protected static void Client_C()
    {
        //成功连接
        MessageHandlerClient.Regist('C',(data) =>
        {
            var i = int.Parse(data.Substring(3, data.Length - 3));
            EnsInstance.LocalClientId = i;
            EnsInstance.OnServerConnect.Invoke();
        });
    }
    protected static void Client_E()
    {
        //事件
        MessageHandlerClient.Regist('E', (data) =>
        {
            string[] s = data.Substring(3, data.Length - 3).Split('#');
            int e = int.Parse(s[0]);
            int i = int.Parse(s[1]);
            if (e == 1) EnsInstance.OnClientEnter?.Invoke(i);
            else if (e == 2) EnsInstance.OnClientExit?.Invoke(i);
            else Debug.LogError("[E]存在错误的事件消息 " + data);
        });
    }
    protected static void Client_H()
    {
        MessageHandlerClient.Regist('H',(data) =>
        {
            EnsInstance.Corr.Client?.SendData(data);
        });
    }
    protected static void Client_D()
    {
        MessageHandlerClient.Regist('D',(data) =>
        {
            EnsInstance.Corr.ShutDown();
        });
    }
    protected static void Client_A()
    {
        MessageHandlerClient.Regist('A',(data) =>
        {
            int d = int.Parse(data.Substring(3, data.Length - 3));
            EnsInstance.HasAuthority = d == 1;
            EnsInstance.OnAuthorityChanged?.Invoke();
        });
    }
    protected static void Client_F()
    {
        MessageHandlerClient.Regist('F',(data) =>
        {
            var s = Format.SplitWithBoundaries(data.Substring(3, data.Length - 3), '#');
            int id = int.Parse(s[0]);
            string func = s[1];
            EnsBehaviour obj = EnsNetworkObjectManager.GetObject(id);
            if (obj == null)
            {
                if (EnsInstance.DevelopmentDebug) Debug.LogError("未找到id为" + id + "的物体");
                return;
            }
            if (s.Count >= 4)
            {
                string param = s[3];
                obj.CallFuncLocal(func, param);
            }
            else
            {
                obj.CallFuncLocal(func);
            }
        });
    }
    protected static void Client_S()
    {
        MessageHandlerClient.Regist('S',(data) =>
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
        });
    }
    protected static void Client_f()
    {
        MessageHandlerClient.Regist('f',(data) =>
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
        });
    }
    protected static void Client_Q()
    {
        MessageHandlerClient.Regist('Q',(data) =>
        {
            if (data[1] == 'Q')
            {
                var s = Format.SplitWithBoundaries(data.Substring(3, data.Length - 3), '#');
                EnsClientRequest.RecvReply(s[0], s[1]);
            }
        });
    }

    protected static void ForceInvokeOnce_Server()
    {
        EnsInstance.OnServerConnect += () =>
        {
            EnsInstance.ServerDisconnectInvoke = false;
            EnsInstance.ClientConnectRejected = false;
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
}