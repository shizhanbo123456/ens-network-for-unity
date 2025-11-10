using System;
using System.Collections.Generic;

public static class EnsServerRequest
{
    private static Dictionary<string,Func<EnsConnection, string,string>> Actions= new Dictionary<string,Func<EnsConnection, string,string>>();
    public static void RegistRequestHeader(string header,Func<EnsConnection,string,string>callback)
    {
        if (Actions.ContainsKey(header)) Actions[header] = callback;
        else Utils.Debug.LogError(header + "已经注册了事件");
    }
    public static string OnRecvRequest(string header,string content,EnsConnection conn)
    {
        if (Actions.ContainsKey(header))
        {
            return Actions[header].Invoke(conn, content);
        }
        else
        {
            Utils.Debug.LogError("未注册的请求头：" + header);
            return string.Empty;
        }
    }
}