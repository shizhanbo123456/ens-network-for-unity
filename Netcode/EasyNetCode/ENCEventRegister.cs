using UnityEngine;
internal class ENCEventRegister
{
    private ENCCorrespondent enc;
    internal virtual void RegistEvents()
    {
        RegistGeneralEvent();
        enc = ENCInstance.ENCCorrespondent;
        Client_C();
        Client_E();
        Client_H();
        Server_H();
        Client_D();
        Server_D();
        W_ConnectFailed();
        W_RecvConnection();
        E_ServerConnected();
        E_ServerDisconnect();
        E_ConnectFailed();
        Client_Any();
        Server_Any();
    }
    protected virtual void Client_C()
    {
        //Id分配
        ENCInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'C')
            {
                enc.ENCClient.ClientId = int.Parse(data.Substring(3, data.Length - 3));
                ENCInstance.LocalClientId = enc.ENCClient.ClientId;
                ENCInstance.OnServerConnect.Invoke(enc.ENCClient.ClientId);
            }
        };
    }
    protected virtual void Client_E()
    {
        //事件
        ENCInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'E')
            {
                string[] s = data.Substring(3, data.Length - 3).Split('#');
                int e = int.Parse(s[0]);
                int i = int.Parse(s[1]);
                if (e == 1) ENCInstance.OnClientConnect?.Invoke(i);
                else if (e == 2) ENCInstance.OnClientDisconnect?.Invoke(i);
                else Debug.LogError("[E]存在错误的事件消息 " + data);
            }
        };
    }
    protected virtual void Client_H()
    {
        ENCInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'H')
            {
                ENCInstance.ENCCorrespondent.ENCClient?.SendData(data);
            }
        };
    }
    protected virtual void Server_H()
    {
        ENCInstance.ServerRecvData += (data,conn) =>
        {
            if (data[1] == 'H')
            {
                ENCInstance.OnRecvResponse(data.Substring(3, data.Length - 3), conn);
            }
        };
    }
    protected virtual void Client_D()
    {
        ENCInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'D')
            {
                //ENCInstance.ENCCorrespondent.ENCClient.hbRecvTime.ReachAt(-1);//服务器已关闭，不需要广播
                ENCInstance.ENCCorrespondent.ShutDown();
            }
        };
    }
    protected virtual void Server_D()
    {
        ENCInstance.ServerRecvData += (data, conn) =>
        {
            if (data[1] == 'D')
            {
                conn.ShutDown();
            }
        };
    }
    protected virtual void W_ConnectFailed()
    {
        ProtocolWrapper.Protocol.OnClientConnectFailed += () => 
        {
            ENCInstance.OnConnectFailed.Invoke(); 
        };
    }
    protected virtual void W_RecvConnection()
    {
        ProtocolWrapper.Protocol.OnRecvConnection += (conn, index) =>
        {
            enc.ENCServer.OnRecvConnection(conn, index);
        };
    }
    protected virtual void E_ServerConnected()
    {
        ENCInstance.OnServerConnect += _ =>
        {
            ENCInstance.DisconnectInvoke = false;
        };
    }
    protected virtual void E_ServerDisconnect()
    {
        //本地事件
        ENCInstance.OnServerDisconnect += () =>
        {
            ENCInstance.LocalClientId = -1;
            ENCInstance.DisconnectInvoke = true;
        };
    }
    protected virtual void E_ConnectFailed()
    {
        ENCInstance.OnConnectFailed += () =>
        {
            enc.ShutDown();
        };
    }
    protected virtual void Client_Any()
    {
        //心跳检测时间重置
        ENCInstance.ClientRecvData += (data) =>
        {
            if (data.Length > 2)
            {
                ENCInstance.ENCCorrespondent.ENCClient.hbRecvTime.ReachAfter(ENCInstance.DisconnectThreshold);
            }
        };
    }
    protected virtual void Server_Any()
    {
        //心跳检测时间重置
        ENCInstance.ServerRecvData += (data, sr) =>
        {
            if (data.Length > 2)
            {
                sr.hbRecvTime.ReachAfter(ENCInstance.DisconnectThreshold);
            }
        };
    }
    protected virtual void RegistGeneralEvent()
    {
        if (ENCInstance.ShowGeneralEvent)
        {
            ENCInstance.OnClientConnect += (id) => { Debug.Log("[E]Event:有新的客户端连接，id=" + id); };
            ENCInstance.OnClientDisconnect += (id) => { Debug.Log("[E]Event:客户端断开连接，id=" + id); };
            ENCInstance.OnServerDisconnect += () => { Debug.Log("[E]Event:和服务器断开连接"); };
            ENCInstance.OnServerConnect += (id) => { Debug.Log("[E]Event:成功连接服务器，Id已分配：" + id); };

            ENCInstance.OnConnectFailed += () => { Debug.Log("[E]连接失败"); };
        }
    }
}