using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class NOMInstance
{
    public static char TargetSeparator = '/';

    public static NOMCorrespondent NOMCorrespondent;
    public static NOMSpawner NOMSpawner;

    public static bool LogOnAllocateId = false;
    public static bool LogOnAutoAssignedId = true;
    public static bool LogOnManualAssignedId = true;

    public static HashSet<int> ManualAssignedId=new HashSet<int>();

    //<0为玩家设置的初始化场景时制造的物体
    private static int sceneobjid = -1;
    public static int AutoSceneObjId
    {
        get
        {
            return sceneobjid--;
        }
    }

    private static readonly IComparer<int> DescendingComparer = Comparer<int>.Create((x, y) =>
    {
        // 核心逻辑：反转默认比较结果（x.CompareTo(y)是升序，y.CompareTo(x)是降序）
        return y.CompareTo(x);
    });
    private static SortedDictionary<int, Dictionary<int,NOMBehaviour>> prioritizedUpdate =
        new SortedDictionary<int, Dictionary<int, NOMBehaviour>>(DescendingComparer);
    private static SortedDictionary<int, Dictionary<int, NOMBehaviour>> prioritizedFixedUpdate =
        new SortedDictionary<int, Dictionary<int, NOMBehaviour>>(DescendingComparer);
    // 原始ID映射表（用于快速查找物体）
    private static Dictionary<int, NOMBehaviour> objectMap = 
        new Dictionary<int, NOMBehaviour>();
    private static Dictionary<int, int> priorityMap =
        new Dictionary<int, int>();
    private static Dictionary<int, int> fixedPriorityMap =
        new Dictionary<int, int>();


    internal static IEnumerable<int> GetPriority()
    {
        return prioritizedUpdate.Keys;
    }
    internal static IEnumerable<int> GetFixedPriority()
    {
        return prioritizedFixedUpdate.Keys;
    }
    /// <summary>
    /// 按优先级执行所有物体的Update
    /// </summary>
    internal static void Update(int priority)
    {
        if (prioritizedUpdate.TryGetValue(priority, out var group))
            foreach(var behaviour in group.Values)
            {
                if (behaviour.nomEnabled)
                {
                    behaviour.ManagedUpdate();
                }
            }
        if (ENCInstance.MessyLog) Debug.Log($"更新了优先级为{priority}的物体");
    }
    /// <summary>
    /// 按优先级执行所有物体的FixedUpdate
    /// </summary>
    internal static void FixedUpdate(int priority)
    {
        if (prioritizedFixedUpdate.TryGetValue(priority, out var group))
            foreach (var behaviour in group.Values)
            {
                if (behaviour.gameObject.activeInHierarchy && behaviour.enabled)
                {
                    behaviour.FixedManagedUpdate();
                }
            }
        if (ENCInstance.MessyLog) Debug.Log($"更新了优先级为{priority}的物体");
    }


    internal static void AddObject(NOMBehaviour behaviour)
    {
        if (behaviour == null)
        {
            Debug.LogWarning("尝试添加空的NOMBehaviour");
            return;
        }
        int objectId = behaviour.ObjectId;
        if (objectMap.ContainsKey(objectId))
        {
            Debug.LogWarning($"id为{objectId}的物体已经被添加");
            return;
        }

        objectMap[objectId] = behaviour; 
        if (GetManagedUpdatePriority(behaviour, out int priority))
        {
            priorityMap[objectId] = priority;
            if (!prioritizedUpdate.ContainsKey(priority))
                prioritizedUpdate[priority] = new Dictionary<int, NOMBehaviour>();
            prioritizedUpdate[priority].Add(behaviour.ObjectId,behaviour);
        }
        if(GetFixedManagedUpdatePriority(behaviour, out priority))
        {
            fixedPriorityMap[objectId] = priority;
            if (!prioritizedFixedUpdate.ContainsKey(priority))
                prioritizedFixedUpdate[priority] = new Dictionary<int, NOMBehaviour>();
            prioritizedFixedUpdate[priority].Add(behaviour.ObjectId, behaviour);
        }
    }
    internal static bool HasObject(int id)
    {
        if (ENCInstance.DevelopmentDebug&&id == NOMSpawner.ObjectId)
        {
            Debug.LogWarning("[N]检查了生成器，请检查代码正确性");
            return true;
        }
        return objectMap.ContainsKey(id);
    }
    internal static NOMBehaviour GetObject(int id)
    {
        if (objectMap.TryGetValue(id, out var data))
            return data;
        return null;
    }
    internal static void RemoveObject(int id)
    {
        if (objectMap.ContainsKey(id))
        {
            objectMap.Remove(id); 
            if (priorityMap.TryGetValue(id, out int updatePriority))
            {
                if (prioritizedUpdate.TryGetValue(updatePriority, out var updateGroup))
                {
                    updateGroup.Remove(id);
                    if (updateGroup.Count == 0)
                        prioritizedUpdate.Remove(updatePriority);
                }
                priorityMap.Remove(id);
            }
            if (fixedPriorityMap.TryGetValue(id, out int fixedPriority))
            {
                if (prioritizedFixedUpdate.TryGetValue(fixedPriority, out var fixedGroup))
                {
                    fixedGroup.Remove(id);
                    if (fixedGroup.Count == 0)
                        prioritizedFixedUpdate.Remove(fixedPriority);
                }
                fixedPriorityMap.Remove(id);
            }
        }
        else
        {
            if(ENCInstance.DevelopmentDebug)Debug.LogWarning($"移除时未找到id为{id}的物体");
            return;
        }
    }
    private static bool GetManagedUpdatePriority(NOMBehaviour behaviour,out int priority)
    {
        priority = 0;

        var method = behaviour.GetType().GetMethod(nameof(NOMBehaviour.ManagedUpdate),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (method == null) return false; // 无此方法

        var ignore = method.GetCustomAttribute<NOMIgnoreAttribute>();
        if (ignore != null) return false;
        var attribute = method.GetCustomAttribute<NOMPriorityAttribute>();
        priority = attribute?.priority ?? 0; // 无标记默认优先级0
        return true;
    }
    private static bool GetFixedManagedUpdatePriority(NOMBehaviour behaviour, out int priority)
    {
        priority = 0;

        var method = behaviour.GetType().GetMethod(nameof(NOMBehaviour.FixedManagedUpdate),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (method == null) return false; // 无此方法

        var ignore = method.GetCustomAttribute<NOMIgnoreAttribute>();
        if (ignore != null) return false;
        var attribute = method.GetCustomAttribute<NOMPriorityAttribute>();
        priority = attribute?.priority ?? 0; // 无标记默认优先级0
        return true;
    }
}