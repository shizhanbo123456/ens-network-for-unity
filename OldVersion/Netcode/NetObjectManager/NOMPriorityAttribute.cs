using System;

[AttributeUsage(AttributeTargets.Method)]
public class NOMPriorityAttribute:Attribute
{
    internal int priority = 0;
    public NOMPriorityAttribute(int priority)
    {
        this.priority = priority;
    }
}