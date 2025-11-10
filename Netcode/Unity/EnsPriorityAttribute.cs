using System;

[AttributeUsage(AttributeTargets.Method)]
public class EnsPriorityAttribute:Attribute
{
    internal int priority = 0;
    public EnsPriorityAttribute(int priority)
    {
        this.priority = priority;
    }
}