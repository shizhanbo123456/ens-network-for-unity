using Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ProtocolWrapper;

public class Format
{
    public static string Combine(CircularQueue<string> origin, char separator)
    {
        if (origin.Empty())
        {
            Debug.Log("传入了空的原始数据");
            return null;
        }
        var result = new StringBuilder();
        bool isFirst = true;
        int length = 0;
        while (!origin.Empty())
        {
            if (!isFirst) result.Append(separator);
            else isFirst = false;

            origin.Read(out string item);
            result.Append(item);
            length=item.Length;
            if (length > 1400)
            {
                Debug.LogError("检查到过长的数据 "+result.ToString());
                break;
            }
        }
        
        return result.ToString();
    }
    public static string[] Split(string origin, char separator)
    {
        return origin.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);
    }


    public static byte[] GetBytes(string s)
    {
        return Encoding.UTF8.GetBytes(s);
    }
    public static string GetString(byte[] b)
    {
        return Encoding.UTF8.GetString(b);
    }
    public static string GetString(byte[] b, int start, int length)
    {
        return Encoding.UTF8.GetString(b, start, length);
    }

    public static string ListToString(List<int> list, char c)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("空列表");
            return "";
        }
        string a = list[0].ToString();
        string cs = c.ToString();
        int i = 1;
        while (i < list.Count)
        {
            a += cs + list[i];
            i++;
        }
        return a;
    }
    public static List<int> StringToList(string a, char c)
    {
        string[] s = a.Split(c,StringSplitOptions.RemoveEmptyEntries);
        List<int> list = new List<int>();
        foreach (var i in s)
        {
            list.Add(int.Parse(i));
        }
        return list;
    }

    public static string EnFormat(string s)
    {
        return Protocol.Separator + s + Protocol.Separator;
    }
    public static string DeFormat(string s,out bool rightFormat)
    {
        rightFormat = false;
        if (string.IsNullOrEmpty(s)) return null;

        int firstSeparatorIndex = s.IndexOf(Protocol.Separator);
        int lastSeparatorIndex = s.LastIndexOf(Protocol.Separator);

        if (firstSeparatorIndex == -1 || lastSeparatorIndex == -1 || firstSeparatorIndex >= lastSeparatorIndex) return null;

        rightFormat= true;
        int length = lastSeparatorIndex - firstSeparatorIndex - 1;
        return s.Substring(firstSeparatorIndex + 1, length);
    }
}