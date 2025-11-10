using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SRCRoomManager:Disposable
{
    private List<SRCRoom> rooms = new List<SRCRoom>();
    private int RoomId = 10000;

    public static SRCRoomManager Instance
    {
        get
        {
            return SRCDedicatedServer.Instance.RoomManager;
        }
    }

    public void Update()
    {
        for (int i = rooms.Count - 1; i >= 0; i--)
        {
            if (rooms[i] == null || !rooms[i].On)
            {
                rooms.RemoveAt(i);
                continue;
            }
            rooms[i].Update();
        }
    }


    //返回值： >0 房间id   ==0 操作失败   -1离开房间
    public int CreateRoom(SRCConnection conn, out int clientId)
    {
        if (conn.room != null)
        {
            if (SRCProgram.Instance.RoomDebug) Debug.Log("[*]尝试创建房间失败");
            clientId = 0;
            return 0;
        }
        rooms.Add(new SRCRoom(RoomId));
        clientId = rooms[rooms.Count - 1].Join(conn);
        RoomId += 1;
        if (SRCProgram.Instance.RoomDebug) Debug.Log("创建了房间 " + (RoomId - 1).ToString());
        return RoomId - 1;
    }
    public int JoinRoom(SRCConnection conn, int id, out int clientId)
    {
        if (conn.room != null)
        {
            if (SRCProgram.Instance.RoomDebug) Debug.Log("[*]尝试加入房间失败");
            clientId = 0;
            return 0;
        }
        foreach (var r in rooms)
        {
            if (id != r.RoomId) continue;
            clientId = r.Join(conn);
            if (SRCProgram.Instance.RoomDebug) Debug.Log("加入了房间 " + id);
            return id;
        }
        clientId = 0;
        if (SRCProgram.Instance.RoomDebug) Debug.Log("无法找到房间 " + id);
        return 0;
    }
    public int ExitRoom(SRCConnection conn, out int ignore)//不需要考量ignore参数
    {
        ignore = 0;
        if (conn.room == null)
        {
            if (SRCProgram.Instance.RoomDebug) Debug.Log("[*]尝试退出房间失败");
            return 0;
        }
        if (conn.room == null) return 0;
        if (SRCProgram.Instance.RoomDebug) Debug.Log("离开了房间 " + conn.room.RoomId);
        if (conn.room.Exit(conn)) return -1;
        else return 0;
    }


    protected override void ReleaseManagedMenory()
    {
        foreach (var r in rooms) r.Dispose();
        rooms.Clear();
        base.ReleaseManagedMenory();
    }
    protected override void ReleaseUnmanagedMenory()
    {
        rooms = null;
        base.ReleaseUnmanagedMenory();
    }





    public string PrintData()
    {
        for (int i = rooms.Count - 1; i >= 0; i--)
        {
            if (!rooms[i].On)
            {
                rooms.RemoveAt(i);
                continue;
            }
            rooms[i].Update();
        }
        string r = "房间信息：";
        foreach (var i in rooms)
        {
            r += i.PrintData() + " ";
        }
        return r;
    }
}
