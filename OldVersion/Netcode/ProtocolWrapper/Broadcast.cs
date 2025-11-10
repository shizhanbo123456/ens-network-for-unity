using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Utils;

namespace ProtocolWrapper
{
    // 存储接收消息的结构体，包含内容和接收时间
    public struct ReceivedMessage
    {
        public string Content;
        public float ReceiveTime;
    }

    public class Broadcast
    {
        public static float broadcastInterval = 1f;
        public static float CleanupExpiredMessagesInterval = 2f;
        public static float messageTimeout = 5f;

        private static ReachTime reachTime = new ReachTime(-1, ReachTime.InitTimeFlagType.ReachAt);
        private static ReachTime CleanExpiredTime= new ReachTime(-1, ReachTime.InitTimeFlagType.ReachAt);
        private static Dictionary<string, string> BroadcastContent = new Dictionary<string, string>();
        // 改为存储多个消息体及其接收时间
        private static Dictionary<string, List<ReceivedMessage>> ReceiveContent = new Dictionary<string, List<ReceivedMessage>>();

        private static UdpClient senderClient;
        private static UdpClient receiverClient;
        private static IPEndPoint broadcastEndPoint;

        public static bool StartBroadcast(int port)
        {
            if (senderClient != null)
            {
                if (Protocol.DevelopmentDebug) Debug.LogWarning("广播已经开启");
                return true;
            }
            try
            {
                senderClient = new UdpClient();
                senderClient.EnableBroadcast = true;
                broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void BroadcastUpdate()
        {
            if (!reachTime.Reached) return;
            reachTime.ReachAfter(broadcastInterval);

            StringBuilder sb = new StringBuilder();

            foreach (var i in BroadcastContent)
            {
                sb.Append(',');
                sb.Append($"{i.Key}:{i.Value}");
            }
            sb.Append(',');

            try
            {
                byte[] data = Format.GetBytes(sb.ToString());
                senderClient.Send(data, data.Length, broadcastEndPoint);
            }
            catch
            {
                EndBroadcast();
                Debug.LogError("广播发送失败，已自动关闭广播");
            }
        }

        public static void EndBroadcast()
        {
            if (senderClient != null)
            {
                senderClient.Close();
                senderClient.Dispose();
                senderClient = null;
                broadcastEndPoint = null;
            }
            else if (Protocol.DevelopmentDebug) Debug.Log("广播发送已经关闭");
        }

        public static void AddInfo(string header, string content)
        {
            if (Protocol.DevelopmentDebug)
            {
                if (header.Contains(':') || header.Contains(',')) Debug.LogError("消息头不合规");
                if (content.Contains(':') || content.Contains(',')) Debug.LogError("消息体不合规");
            }
            // 避免重复添加相同键（如果需要允许重复键不同值，可删除此判断）
            if (BroadcastContent.ContainsKey(header))
                BroadcastContent[header] = content;
            else
                BroadcastContent.Add(header, content);
        }

        public static void RemoveInfo(string header)
        {
            if (BroadcastContent.ContainsKey(header))
                BroadcastContent.Remove(header);
            else if (Protocol.DevelopmentDebug)
                Debug.LogWarning($"广播消息不存在:{header}");
        }

        public static void ClearSendContent()
        {
            BroadcastContent.Clear();
        }

        public static bool StartRecv(int port)
        {
            if (receiverClient != null)
            {
                Debug.LogWarning("接收已经开启");
                return true;
            }
            try
            {
                receiverClient = new UdpClient(port);
                receiverClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void RecvUpdate()
        {
            try
            {
                while (receiverClient.Available > 0)
                {
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedData = receiverClient.Receive(ref remoteEndPoint);
                    string message = Format.GetString(receivedData);

                    int msgstart = message.IndexOf(',');
                    int msgend = message.LastIndexOf(",");
                    if (msgstart == -1 || msgend == -1 || msgend <= msgstart) continue;

                    string msg = message.Substring(msgstart, msgend - msgstart + 1);
                    string[] parts = msg.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var part in parts)
                    {
                        HandleReceivedMesg(part);
                    }
                }
            }
            catch
            {
                EndRecv();
                Debug.LogError("接收广播失败，已自动关闭广播");
            }
        }
        private static void HandleReceivedMesg(string mesg)
        {
            string[] keyValue = mesg.Split(':');
            if (keyValue.Length != 2)
            {
                Debug.LogWarning("异常的广播信息：" + mesg);
                return;
            }

            string header = keyValue[0];
            string content = keyValue[1];

            // 存储消息（包含时间戳消息）
            var receivedMsg = new ReceivedMessage
            {
                Content = content,
                ReceiveTime = Utils.Time.time // 记录接收时的本地时间
            };

            if (ReceiveContent.ContainsKey(header))
            {
                bool exists = false;
                for (int i = 0; i < ReceiveContent[header].Count; i++)
                {
                    ReceivedMessage item = ReceiveContent[header][i];
                    if (item.Content == content)
                    {
                        exists = true;
                        ReceiveContent[header][i] = receivedMsg;
                        break;
                    }
                }
                if (!exists)
                {
                    ReceiveContent[header].Add(receivedMsg);
                }
            }
            else
            {
                ReceiveContent[header] = new List<ReceivedMessage> { receivedMsg };
            }
        }

        // 清理超时消息
        private static void CleanupExpiredMessages()
        {
            float currentTime = Utils.Time.time;
            List<string> headersToRemove = new List<string>();

            foreach (var kvp in ReceiveContent)
            {
                // 移除列表中超时的消息
                kvp.Value.RemoveAll(msg => currentTime - msg.ReceiveTime > messageTimeout);

                // 如果列表为空，标记整个键移除
                if (kvp.Value.Count == 0)
                {
                    headersToRemove.Add(kvp.Key);
                }
            }

            // 移除空列表的键
            foreach (var header in headersToRemove)
            {
                ReceiveContent.Remove(header);
            }
        }

        public static void EndRecv()
        {
            if (receiverClient != null)
            {
                receiverClient.Close();
                receiverClient.Dispose();
                receiverClient = null;
            }
            else if (Protocol.DevelopmentDebug)
                Debug.Log("广播接收已经关闭");
        }

        // 修改获取内容的方法，返回所有相关消息
        public static bool TryGetContents(string header, out List<ReceivedMessage> contents)
        {
            return ReceiveContent.TryGetValue(header, out contents);
        }

        public static void ClearRecvContent()
        {
            ReceiveContent.Clear();
        }

        public static void Update()
        {
            if (senderClient != null)
                BroadcastUpdate();

            if (receiverClient != null)
                RecvUpdate();

            if (CleanExpiredTime.Reached)
            {
                CleanExpiredTime.ReachAfter(CleanupExpiredMessagesInterval);
                CleanupExpiredMessages();
            }
        }
    }
}