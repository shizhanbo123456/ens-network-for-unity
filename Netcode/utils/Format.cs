using System;
using System.Collections.Generic;
using System.Text;
using Utils;

public static class Format
{
    private const char BoundaryStart = '{';
    private const char BoundaryEnd = '}';
    private const char Separator = '*';
    private const char TargetSeparator = '/';
    public static List<string> SplitWithBoundaries(string s, char separator=Separator, char boundaryStart=BoundaryStart, char boundaryEnd=BoundaryEnd)
    {
        List<string> result = new List<string>();
        int currentStart = 0;
        int boundaryDepth = 0; // 用于处理嵌套边界的情况

        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];

            // 处理边界开始符
            if (c == boundaryStart)
            {
                boundaryDepth++;
            }
            // 处理边界结束符
            else if (c == boundaryEnd)
            {
                if (boundaryDepth > 0)
                {
                    boundaryDepth--;
                }
                // 忽略不匹配的边界结束符
            }
            // 当遇到分隔符且不在边界内时进行分割
            else if (c == separator && boundaryDepth == 0)
            {
                // 提取当前片段并处理边界
                string segment = s.Substring(currentStart, i - currentStart);
                string processedSegment = ProcessBoundaries(segment, boundaryStart, boundaryEnd);
                result.Add(processedSegment);

                currentStart = i + 1; // 移动到下一个片段的起始位置
            }
        }

        // 添加最后一个片段
        if (currentStart <= s.Length - 1)
        {
            string lastSegment = s.Substring(currentStart);
            string processedLastSegment = ProcessBoundaries(lastSegment, boundaryStart, boundaryEnd);
            result.Add(processedLastSegment);
        }

        return result;
    }
    // 处理片段中的边界符，移除最外层的边界
    private static string ProcessBoundaries(string segment, char boundaryStart, char boundaryEnd)
    {
        // 检查是否同时包含完整的边界符
        if (segment.Length >= 2 && segment[0] == boundaryStart && segment[segment.Length - 1] == boundaryEnd)
        {
            // 移除最外层的边界符
            return segment.Substring(1, segment.Length - 2);
        }
        return segment;
    }



    public static string ListToString(List<int> list, char c=TargetSeparator)
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
    public static List<int> StringToList(string a, char c=TargetSeparator)
    {
        string[] s = a.Split(c, StringSplitOptions.RemoveEmptyEntries);
        List<int> list = new List<int>();
        foreach (var i in s)
        {
            list.Add(int.Parse(i));
        }
        return list;
    }


    //Wrapper
    public static string Combine(CircularQueue<string> origin)
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
            if (!isFirst) result.Append(Separator);
            else isFirst = false;

            origin.Read(out string item);
            result.Append(item);
            length = item.Length;
            if (length > 1400)
            {
                Debug.LogError("检查到过长的数据 " + result.ToString());
                break;
            }
        }

        return result.ToString();
    }
    public static string[] Split(string origin)
    {
        return origin.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
    }


    //Wrappper
    public static string EnFormat(string s)
    {
        return Separator + s + Separator;
    }
    public static string DeFormat(string s, out bool rightFormat)
    {
        rightFormat = false;
        if (string.IsNullOrEmpty(s)) return null;

        int firstSeparatorIndex = s.IndexOf(Separator);
        int lastSeparatorIndex = s.LastIndexOf(Separator);

        if (firstSeparatorIndex == -1 || lastSeparatorIndex == -1 || firstSeparatorIndex >= lastSeparatorIndex) return null;

        rightFormat = true;
        int length = lastSeparatorIndex - firstSeparatorIndex - 1;
        return s.Substring(firstSeparatorIndex + 1, length);
    }

    //Wrapper
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
}