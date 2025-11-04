using UnityEngine;
internal class SRCEventRegister:NOMEventRegister
{
    private SRCCorrespondent src;
    internal override void RegistEvents()
    {
        base.RegistEvents();
        src = SRCInstance.SRCCorrespondent;
        Client_R();
        S_ServerConnected();
        S_ServerDisconnected();
    }
    protected virtual void Client_R()
    {
        //处理返回的房间信息
        ENCInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'R')
            {
                string[] s = data.Substring(3, data.Length - 3).Split('#');
                int roomId = int.Parse(s[0]);
                int clientId = int.Parse(s[1]);
                if (roomId > 0)//成功加入
                {
                    if (ENCInstance.DevelopmentDebug) Debug.Log("[S]加入了房间");
                    ENCInstance.ENCCorrespondent.ENCClient.ClientId = clientId;
                    SRCInstance.PresentRoomId = roomId;
                    SRCInstance.SRCHost = clientId == 0;
                    ENCInstance.OnServerConnect.Invoke(clientId);
                }
                else if (roomId == -1)//成功离开
                {
                    if (ENCInstance.DevelopmentDebug) Debug.Log("[S]离开了房间");
                    ENCInstance.OnServerDisconnect.Invoke();
                    SRCInstance.PresentRoomId = 0;
                    SRCInstance.SRCHost = false;
                }
                else if (roomId == 0)
                {
                    if (ENCInstance.DevelopmentDebug) Debug.LogError("[S]加入房间失败");
                    SRCInstance.OnJoinFailed?.Invoke();
                }
                else Debug.LogError("[S]错误信息 " + data);
            }
        };
    }
    protected virtual void S_ServerConnected()
    {
        SRCInstance.OnServerConnected += () =>
        {
            SRCInstance.DisconnectInvoke = false;
        };
    }
    protected virtual void S_ServerDisconnected()
    {
        SRCInstance.OnServerDisconnect += () =>
        {
            SRCInstance.DisconnectInvoke = true;
        };
    }
    protected override void Client_C()
    {
        //此处需要将E层成功连接服务器的消息"C"改为S层连接服务器
        //E层的连接到服务器由加入房间时触发
        
        //成功连接
        ENCInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'C')
            {
                if (SRCInstance.SRCOn)
                {
                    SRCInstance.OnServerConnected.Invoke();
                }
                else
                {
                    src.ENCClient.ClientId = int.Parse(data.Substring(3, data.Length - 3));
                    ENCInstance.OnServerConnect.Invoke(src.ENCClient.ClientId);
                }
            }
        };
    }
    protected override void RegistGeneralEvent()
    {
        if (ENCInstance.ShowGeneralEvent)
        {
            ENCInstance.OnClientConnect += (id) => { Debug.Log("[S]Event:有新的客户端加入房间，id=" + id); };
            ENCInstance.OnClientDisconnect += (id) => { Debug.Log("[S]Event:客户端离开房间，id=" + id); };
            ENCInstance.OnServerDisconnect += () => { Debug.Log("[S]Event:已离开房间"); };
            ENCInstance.OnServerConnect += (id) => { Debug.Log("[S]Event:成功加入房间，Id已分配：" + id); };

            SRCInstance.OnServerConnected += () => { Debug.Log("[S]Event:SRC已连接到远程服务器"); };
            SRCInstance.OnServerDisconnect += () => { Debug.Log("[S]Event:SRC已和远程服务器断开连接"); };

            ENCInstance.OnConnectFailed += () => { Debug.Log("[S]连接失败"); };
        }
    }
}