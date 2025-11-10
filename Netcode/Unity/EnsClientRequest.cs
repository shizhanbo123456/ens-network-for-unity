using System;
using System.Collections.Generic;
using Utils;

public static class EnsClientRequst
{
    private static Dictionary<string, Action<string>> CallbackEvents = new Dictionary<string, Action<string>>();
    private static Dictionary<string, Action> TimeoutEvents = new Dictionary<string, Action>();

    private static Dictionary<string,float>ActiveRequestHeader = new Dictionary<string, float>();

    public static void RegistRequest(string header, Action<string> callback,Action timeout=null)
    {
        if (CallbackEvents.ContainsKey(header)) CallbackEvents[header] += callback;
        else CallbackEvents.Add(header, callback);
        
        if (timeout == null) return;
        if (TimeoutEvents.ContainsKey(header)) TimeoutEvents[header] += timeout;
        else TimeoutEvents.Add(header, timeout);
    }
    public static bool SendRequest(string header,string content,bool keyValue=true)
    {
        if (ActiveRequestHeader.ContainsKey(header)) return false;
        if (EnsInstance.Corr == null) return false;
        if (EnsInstance.Corr.Client == null) return false;
        EnsInstance.Corr.Client.SendData(keyValue ? Header.kQ : Header.Q + "{" + header + "}#{" + content + "}");
        ActiveRequestHeader.Add(header, Time.time + EnsInstance.KeyExistTime + 1);
        return true;
    }
    public static void RecvReply(string header,string content)
    {
        ActiveRequestHeader.Remove(header);
        CallbackEvents[header].Invoke(content);
    }
    public static void Update()
    {
        List<string>timeExceedKeys=new List<string>();
        foreach(var pair in ActiveRequestHeader)
        {
            if (pair.Value < Time.time)
            {
                timeExceedKeys.Add(pair.Key);
            }
        }
        foreach(var i in timeExceedKeys)
        {
            ActiveRequestHeader.Remove(i);
            if (TimeoutEvents.ContainsKey(i)) TimeoutEvents[i].Invoke();
        }
    }
}