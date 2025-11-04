using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SRCRoom:Disposable
{
    private List<SRCConnection> ClientConnections = new List<SRCConnection>();
    public int RoomId;
    private int ClientId = 0;

    private int createdid = 1;
    public int CreatedId
    {
        get
        {
            return createdid;
        }
        set
        {
            createdid = value;
        }
    }

    public bool On = true;

    private SRCRoom() { }
    public SRCRoom(int id)
    {
        RoomId = id;
    }

    public int Join(SRCConnection conn)
    {
        ClientConnections.Add(conn);
        conn.room = this;
        conn.ClientId = ClientId;
        ClientId++;
        Broadcast("kE]1#" + conn.ClientId, conn.ClientId);
        return conn.ClientId;
    }
    public bool Exit(SRCConnection conn)
    {
        for (int i = ClientConnections.Count - 1; i >= 0; i--)
        {
            if (ClientConnections[i].ClientId == conn.ClientId)
            {
                if (conn.ClientId == 0)//房主离开后房间关闭
                {
                    ClientConnections.RemoveAt(i);
                    Broadcast("kR]-1#0");
                    On = false;
                    continue;
                }
                ClientConnections.RemoveAt(i);
                Broadcast("kE]2#" + conn.ClientId);
                conn.room = null;
                conn.ClientId = -1;
                return true;
            }
        }
        return false;
    }
    public bool HandleData(string data, int clientId)//处理仅在房间内的信息，返回是否成功处理
    {
        try
        {
            if (data[1] == 'F')
            {
                string[] s = data.Substring(3, data.Length - 3).Split('#');
                string target = s[2];
                if (target[0] == '-')
                {
                    if (target[1] == '1')//全部
                    {
                        Broadcast(data);
                    }
                    else if (target[1] == '2')//忽略自身
                    {
                        Broadcast(data, clientId);
                    }
                }
                else
                {
                    PTP(data, Format.StringToList(target, '/'));
                }
            }
            else if (data[1] == 'S')
            {
                ENCConnection connection=null;
                foreach (var i in ClientConnections) if (i.ClientId == clientId) { connection = i;break; }
                if (connection == null) return false;

                string[] s = data.Substring(3, data.Length - 3).Split('#');
                string target = s[2];
                if (target[0] == '-')
                {
                    if (target[1] == '1')//全部
                    {
                        foreach (var i in ClientConnections)
                        {
                            SendTo(s, connection.delay, i);
                        }
                    }
                    else if (target[1] == '2')//忽略自身
                    {
                        foreach (var i in ClientConnections)
                        {
                            if (i.ClientId == connection.ClientId) continue;
                            SendTo(s, connection.delay, i);
                        }
                    }
                }
                else
                {
                    var targets = Format.StringToList(target, '/');
                    foreach (var i in ClientConnections)
                    {
                        if (targets.Contains(i.ClientId))
                            SendTo(s, connection.delay, i);
                    }
                }
            }
            else if (data[1] == 'I')
            {
                Broadcast(data);
            }
            else if (data[1] == 'f')
            {
                string[] s = data.Substring(3, data.Length - 3).Split('#');
                string target = s[2];
                int behaviourCount = int.Parse(s[4]);
                data = data[0] + "f]" + s[0] + "#" + s[1] + "#" + s[2] + "#" + s[3] + "#" + CreatedId;
                CreatedId += behaviourCount;
                if (target[0] == '-')
                {
                    if (target[1] == '1')//全部
                    {
                        Broadcast(data);
                    }
                    else if (target[1] == '2')//忽略自身
                    {
                        Broadcast(data, clientId);
                    }
                }
                else
                {
                    PTP(data, Format.StringToList(target, '/'));
                }
            }
            else if (data[1] == 'D')
            {
                for (int index = 0; index < ClientConnections.Count; index++)
                {
                    SRCConnection conn = ClientConnections[index];
                    if (conn.ClientId == clientId)
                    {
                        conn.ShutDown();
                        conn.Dispose();
                        ClientConnections.RemoveAt(index);
                        break;
                    }
                }
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("异常的函数调用数据 " + data + " " + e.ToString());
        }
        return false;
    }
    private static void SendTo(string[] s, int senderDelay, ENCConnection target)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("IF]");
        for (int i = 0; i < s.Length; i++)
        {
            if (i != 0) sb.Append('#');
            if (i != 3) sb.Append(s[i]);
            else sb.Append((int.Parse(s[3]) - senderDelay - target.delay).ToString());
        }
        target.SendData(sb.ToString());
    }

    public void Broadcast(string data)
    {
        foreach (var i in ClientConnections) i.SendData(data);
    }
    public void Broadcast(string data, int self)
    {
        foreach (var i in ClientConnections) if (i.ClientId != self) i.SendData(data);
    }
    public void PTP(string data, int id)
    {
        foreach (var i in ClientConnections) if (id == i.ClientId) i.SendData(data);
    }
    public void PTP(string data, List<int> id)
    {
        foreach (var i in ClientConnections) if (id.Contains(i.ClientId)) i.SendData(data);
    }




    public void Update()
    {
        for (int i = ClientConnections.Count - 1; i >= 0; i--)
        {
            if (ClientConnections[i].On() == false)
            {
                ClientConnections[i].Dispose();
                ClientConnections.RemoveAt(i);
            }
        }
    }


    protected override void ReleaseManagedMenory()
    {
        foreach(var i in ClientConnections) i.Dispose();
        ClientConnections.Clear();
        base.ReleaseManagedMenory();
    }
    protected override void ReleaseUnmanagedMenory()
    {
        ClientConnections = null;
        base.ReleaseUnmanagedMenory();
    }


    public string PrintData()
    {
        string t = "[ " + RoomId.ToString() + " : ";
        bool first = true;
        foreach (var i in ClientConnections)
        {
            if (!first) t += ",";
            t += i.ClientId;
            first = false;
        }
        t += "]";
        return t;
    }
}