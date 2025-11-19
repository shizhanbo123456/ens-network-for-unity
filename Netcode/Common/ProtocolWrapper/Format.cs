using System;
using System.Text;
using Utils;

namespace ProtocolWrapper
{
    public class Format
    {
        private const char DataSeparator = '*';
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
                if (!isFirst) result.Append(DataSeparator);
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
            return origin.Split(new char[]{DataSeparator}, StringSplitOptions.RemoveEmptyEntries);
        }


        //Wrappper
        public static string EnFormat(string s)
        {
            return DataSeparator + s + DataSeparator;
        }
        public static string DeFormat(string s, out bool rightFormat)
        {
            rightFormat = false;
            if (string.IsNullOrEmpty(s)) return null;

            int firstSeparatorIndex = s.IndexOf(DataSeparator);
            int lastSeparatorIndex = s.LastIndexOf(DataSeparator);

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
}