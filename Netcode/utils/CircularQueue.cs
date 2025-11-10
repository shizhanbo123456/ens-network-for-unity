using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可以有效防止同时进行读取和写入
/// </summary>
public class CircularQueue<T>
{
    private T[] x;
    private int size = 300;

    private int r;
    public int ReadIndex
    {
        get
        {
            return r;
        }
        set
        {
            r = value;
            if (r >= size) r %= size;
        }
    }
    private int w;
    public int WriteIndex
    {
        get { return w; }
        set
        {
            w = value;
            if (w >= size) w %= size;
        }
    }
    public int Count { get; private set; }

    public CircularQueue()
    {
        x = new T[size];
    }
    public CircularQueue(int size)
    {
        x = new T[size];
    }
    //返回值表示是否有效
    public bool Write(T data)
    {
        if (Full())
        {
            Debug.LogError("数据溢出");
            return false;
        }
        else
        {
            x[w] = data;
            WriteIndex += 1;
            Count++;
            return true;
        }
    }
    public bool Read(out T data)//返回表示是否为有效数据
    {
        if (Empty())
        {
            data = default;
            return false;
        }
        else
        {
            data = x[r];
            x[r] = default;
            ReadIndex += 1;
            Count--;
            return true;
        }
    }
    public bool Empty()
    {
        return Count == 0;
    }
    public bool Full()
    {
        return Count == size;
    }
}