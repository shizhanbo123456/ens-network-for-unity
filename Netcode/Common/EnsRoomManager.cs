using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnsRoomManager:Disposable
{
    public static EnsRoomManager Instance;
    public Dictionary<int,EnsRoom> rooms = new Dictionary<int, EnsRoom>();
    private int RoomId = 10000;

    public enum InvalidRoomOperation
    {
        CreatrOrJoinFailed_AlreadyInRoom=-1,
        JoinRoomFailed_CannotFindRoom=-2,
        ExitRoomFailed_NotInRoom=-3
    }

    public EnsRoomManager()
    {
        Instance = this;
    }

    //返回值： >0 房间id   ==0 操作失败   -1离开房间
    public int CreateRoom(EnsConnection conn)
    {
        if (conn.room != null) return (int)InvalidRoomOperation.CreatrOrJoinFailed_AlreadyInRoom;
        rooms.Add(RoomId,new EnsRoom(RoomId));
        rooms[RoomId].Join(conn);
        RoomId += 1;
        if (EnsProgram.Instance.RoomDebug) Debug.Log("创建了房间 " + (RoomId - 1).ToString());
        return conn.room.RoomId;
    }
    public int JoinRoom(EnsConnection conn, int id)
    {
        if (conn.room != null) return (int)InvalidRoomOperation.CreatrOrJoinFailed_AlreadyInRoom;
        if (rooms.ContainsKey(id))
        {
            rooms[id].Join(conn);
            if (EnsProgram.Instance.RoomDebug) Debug.Log("加入了房间 " + id);
            return id;
        }
        else
        {
            if (EnsProgram.Instance.RoomDebug) Debug.Log("无法找到房间 " + id);
            return (int)InvalidRoomOperation.JoinRoomFailed_CannotFindRoom;
        }
    }
    public int ExitRoom(EnsConnection conn)
    {
        if (conn.room == null) return (int)InvalidRoomOperation.ExitRoomFailed_NotInRoom;
        if (EnsProgram.Instance.RoomDebug) Debug.Log("离开了房间 " + conn.room.RoomId);
        conn.room.Exit(conn);
        return 0;
    }







    protected override void ReleaseManagedMenory()
    {
        foreach (var r in rooms.Values) r.Dispose();
        rooms.Clear();
        base.ReleaseManagedMenory();
    }
    protected override void ReleaseUnmanagedMenory()
    {
        Instance = null;
        rooms = null;
        base.ReleaseUnmanagedMenory();
    }
    public string PrintData()
    {
        string r = "房间信息：";
        foreach (var i in rooms)
        {
            r += i.ToString() + " ";
        }
        return r;
    }
}
