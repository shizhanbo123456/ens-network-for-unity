using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class EnsRoom:Disposable
{
    private Dictionary<int,EnsConnection> ClientConnections =new Dictionary<int, EnsConnection>();
    public int RoomId;
    public int CurrentAuthorityAt = -1;

    public bool RoomPublic = true;
    public Dictionary<string,string> Rule=new Dictionary<string,string>();

    // >0为游戏过程中制造的物体的Id
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

    private EnsRoom() { }
    public EnsRoom(int id)
    {
        RoomId = id;
    }

    public void Join(EnsConnection conn)
    {
        ClientConnections.Add(conn.ClientId,conn);
        conn.room = this;
        Broadcast(Header.kE+"1#" + conn.ClientId, conn.ClientId);
        if (CurrentAuthorityAt == -1) SetAuthority(conn.ClientId);
    }
    public bool Exit(EnsConnection conn)
    {
        ClientConnections.Remove(conn.ClientId);
        conn.room = null;
        conn.SendData(Header.kR + "0");
        if (conn.ClientId == CurrentAuthorityAt)
        {
            Broadcast(Header.kE + "2#" + conn.ClientId);
            Broadcast(Header.kR + "0");
            ShutDown();
            return true;
        }
        else if (ClientConnections.Count == 0)//房主离开后房间关闭
        {
            ShutDown();
            return true;
        }
        else
        {
            Broadcast(Header.kE + "2#" + conn.ClientId);
            return true;
        }
    }
    public void SetAuthority(int clientId)
    {
        if (!ClientConnections.ContainsKey(CurrentAuthorityAt)) return;
        if (ClientConnections.ContainsKey(CurrentAuthorityAt))
        {
            ClientConnections[CurrentAuthorityAt].SendData(Header.kA+"0");
        }
        CurrentAuthorityAt= clientId;
        ClientConnections[CurrentAuthorityAt].SendData(Header.kA+"1");
    }

    public void Broadcast(string data)
    {
        foreach (var i in ClientConnections.Values) i.SendData(data);
    }
    public void Broadcast(string data, int self)
    {
        foreach (var i in ClientConnections.Values) if (i.ClientId != self) i.SendData(data);
    }
    public void PTP(string data, int id)
    {
        foreach (var i in ClientConnections.Values) if (id == i.ClientId) i.SendData(data);
    }
    public void PTP(string data, List<int> id)
    {
        foreach (var i in ClientConnections.Values) if (id.Contains(i.ClientId)) i.SendData(data);
    }

    public void ShutDown()
    {
        EnsRoomManager.Instance.rooms.Remove(RoomId);
        Dispose();
    }



    protected override void ReleaseManagedMenory()
    {
        foreach(var i in ClientConnections.Values) i.Dispose();
        ClientConnections.Clear();
        base.ReleaseManagedMenory();
    }
    protected override void ReleaseUnmanagedMenory()
    {
        ClientConnections = null;
        base.ReleaseUnmanagedMenory();
    }

    public override string ToString()
    {
        string t = "[ " + RoomId.ToString() + " : ";
        bool first = true;
        foreach (var i in ClientConnections.Values)
        {
            if (!first) t += ",";
            t += i.ClientId;
            first = false;
        }
        t += "]";
        return t;
    }
}