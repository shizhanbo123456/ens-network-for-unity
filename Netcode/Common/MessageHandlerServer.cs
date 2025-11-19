using System;
using System.Collections.Generic;
using Utils;

public static class MessageHandlerServer
{
    private static Action<string, EnsConnection> AnyHeader;
    private static Dictionary<char, Action<string, EnsConnection>> Events = new Dictionary<char, Action<string, EnsConnection>>();
    public static void RegistAny(Action<string, EnsConnection> action)
    {
        AnyHeader += action;
    }
    public static void Regist(char header, Action<string, EnsConnection> action)
    {
        if (!Events.ContainsKey(header))
        {
            Debug.Log("重复注册消息头：" + header);
            return;
        }
        Events.Add(header, action);
    }
    public static void Invoke(string msg,EnsConnection conn)
    {
        char header = msg[1];
        if (!Events.ContainsKey(header))
        {
            Debug.Log("未注册消息头：" + header);
            return;
        }
        AnyHeader.Invoke(msg,conn);
        Events[header].Invoke(msg,conn);
    }
}