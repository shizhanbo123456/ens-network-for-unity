using System;
using System.Collections.Generic;

public interface IProtocolCommandBuffer
{
    void InMainThread();
}
public class ProtocolEventBuffer:IProtocolCommandBuffer
{
    private Action action;
    private bool trigger = false;

    public ProtocolEventBuffer()
    {
        if (ProtocolWrapper.Protocol.Triggers == null) ProtocolWrapper.Protocol.Triggers = new List<IProtocolCommandBuffer>();
        ProtocolWrapper.Protocol.Triggers.Add(this);
    }
    public void SetTrigger()
    {
        trigger = true;
    }
    public void InMainThread()
    {
        if (trigger)
        {
            try
            {
                action?.Invoke();
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
        trigger = false;
    }
    public static ProtocolEventBuffer operator +(ProtocolEventBuffer origin, Action action)
    {
        origin.action += action;
        return origin;
    }
}