using System;
using System.Collections.Generic;
using Utils;

public static class MessageHandlerClient
{
    private static Action<string> AnyHeader;
    private static Dictionary<char,Action<string>>Events=new Dictionary<char,Action<string>>();
    public static void RegistAny(Action<string> action)
    {
        AnyHeader += action;
    }
    public static void Regist(char header,Action<string> action)
    {
        if (!Events.ContainsKey(header))
        {
            Debug.Log("重复注册消息头：" + header);
            return;
        }
        Events.Add(header,action);
    }
    public static void Invoke(string msg)
    {
        char header = msg[1];
        if (!Events.ContainsKey(header))
        {
            Debug.Log("未注册消息头：" + header);
            return;
        }
        AnyHeader.Invoke(msg);
        Events[header].Invoke(msg);
    }
}