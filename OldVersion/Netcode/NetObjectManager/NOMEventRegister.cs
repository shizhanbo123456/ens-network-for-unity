using ProtocolWrapper;
using System.Text;
using UnityEngine;
internal class NOMEventRegister:ENCEventRegister
{
    private NOMCorrespondent nom;
    internal override void RegistEvents()
    {
        base.RegistEvents();
        nom = NOMInstance.NOMCorrespondent;
        Server_F();
        Client_F();
        Immediate_F();
        Server_f();
        Client_f();
    }
    protected virtual void Server_F()
    {
        ENCInstance.ServerRecvData += (data, connection) =>
        {
            if (data[1] == 'F')
            {
                string[] s = data.Substring(3, data.Length - 3).Split('#');
                string target = s[2];
                if (target[0] == '-')
                {
                    if (target[1] == '1')//全部
                    {
                        nom.ENCServer.Broadcast(data);
                    }
                    else if (target[1] == '2')//忽略自身
                    {
                        nom.ENCServer.Broadcast(data, connection.ClientId);
                    }
                }
                else
                {
                    nom.ENCServer.PTP(data, Format.StringToList(target, '/'));
                }
            }
        };
    }
    protected virtual void Server_S()
    {
        ENCInstance.ServerRecvData += (data, connection) =>
        {
            if (data[1] == 'S')
            {
                string[] s = data.Substring(3, data.Length - 3).Split('#');
                string target = s[2];
                if (target[0] == '-')
                {
                    if (target[1] == '1')//全部
                    {
                        foreach (var i in nom.ENCServer.ClientConnections)
                        {
                            SendTo(s, connection.delay, i);
                        }
                    }
                    else if (target[1] == '2')//忽略自身
                    {
                        foreach (var i in nom.ENCServer.ClientConnections)
                        {
                            if (i.ClientId == connection.ClientId) continue;
                            SendTo(s, connection.delay, i);
                        }
                    }
                }
                else
                {
                    var targets = Format.StringToList(target, '/');
                    foreach (var i in nom.ENCServer.ClientConnections)
                    {
                        if (targets.Contains(i.ClientId))
                            SendTo(s, connection.delay, i);
                    }
                }
            }
        };
    }
    protected virtual void Client_F()
    {
        ENCInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'F')
            {
                string[] s = data.Substring(3, data.Length - 3).Split('#');
                int id = int.Parse(s[0]);
                string func = s[1];
                NOMBehaviour obj = NOMInstance.GetObject(id);
                if (obj == null)
                {
                    if (ENCInstance.DevelopmentDebug) Debug.LogWarning("[N]无物体id=" + id);
                    return;
                }
                if (s.Length >= 4)
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
    protected virtual void Client_S()
    {
        ENCInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'S')
            {
                string[] s = data.Substring(3, data.Length - 3).Split('#');
                int id = int.Parse(s[0]);
                string func = s[1];
                NOMBehaviour obj = NOMInstance.GetObject(id);
                if (obj == null)
                {
                    if (ENCInstance.DevelopmentDebug) Debug.LogWarning("[N]无物体id=" + id);
                    return;
                }
                obj.DelayInvoke(s);
            }
        };
    }
    protected void Immediate_F()
    {
        Protocol.ClientRecvImmediateInvoke += data =>
        {
            if (data[0] == 'i' && data[1] == 'F')
            {
                string[] s = data.Substring(3, data.Length - 3).Split('#');
                int id = int.Parse(s[0]);
                string func = s[1];
                NOMBehaviour obj = NOMInstance.GetObject(id);
                if (obj == null)
                {
                    if (ENCInstance.DevelopmentDebug) Debug.LogWarning("[N]无物体id=" + id);
                    return true;
                }
                if (s.Length >= 4)
                {
                    string param = s[3];
                    obj.CallFuncLocal(func, param);
                }
                else
                {
                    obj.CallFuncLocal(func);
                }
                return true;
            }
            else return false;
        };
    }
    protected virtual void Server_f()
    {
        //物体Id同步
        ENCInstance.ServerRecvData += (data, connection) =>
        {
            if (data[1] == 'f')
            {
                string[] s = data.Substring(3, data.Length - 3).Split('#');
                string target = s[2];
                int behaviourCount = int.Parse(s[4]);
                data = data[0] + "f]" + s[0] + "#" + s[1] + "#" + s[2] + "#" + s[3] + "#" + NOMServer.nomInstance.CreatedId;
                NOMServer.nomInstance.CreatedId += behaviourCount;
                if (target[0] == '-')
                {
                    if (target[1] == '1')//全部
                    {
                        nom.ENCServer.Broadcast(data);
                    }
                    else if (target[1] == '2')//忽略自身
                    {
                        nom.ENCServer.Broadcast(data, connection.ClientId);
                    }
                }
                else
                {
                    nom.ENCServer.PTP(data, Format.StringToList(target, '/'));
                }
            }
        };
    }
    protected virtual void Client_f()
    {
        ENCInstance.ClientRecvData += (data) =>
        {
            if (data[1] == 'f')
            {
                string[] s = data.Substring(3, data.Length - 3).Split('#');
                int id = int.Parse(s[0]);
                //string func = s[1];
                NOMSpawner obj = NOMInstance.NOMSpawner;
                int idStart = int.Parse(s[s.Length - 1]);
                string param = s[3];
                obj.Create(param, idStart);
            }
        };
    }


    private static void SendTo(string[] s,int senderDelay,ENCConnection target)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("IF]");
        for (int i = 0; i < s.Length; i++)
        {
            if (i != 0) sb.Append('#');
            if(i!=3)sb.Append(s[i]);
            else sb.Append((int.Parse(s[3])-senderDelay-target.delay).ToString());
        }
        target.SendData(sb.ToString());
    }
}