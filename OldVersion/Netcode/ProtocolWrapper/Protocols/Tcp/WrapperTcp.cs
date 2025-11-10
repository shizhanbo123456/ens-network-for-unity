using Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ProtocolWrapper.Protocols.Tcp
{
    /// <summary>
    /// 接收时报错会调用OnClientReceiveFailed
    /// </summary>
    internal abstract class WrapperTcp : ProtocolBase
    {
        protected TcpClient Client;
        protected NetworkStream Stream;

        protected void Init(TcpClient client)
        {
            Client = client;
            Stream = client.GetStream();
            Init((Client.Client.RemoteEndPoint as IPEndPoint).Address.ToString(), (Client.Client.RemoteEndPoint as IPEndPoint).Port);
        }
        protected virtual void Receive()
        {
            byte[] buffer = new byte[Protocol.BufferLength];
            while (On)
            {
                try
                {
                    int bytesRead = Stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) continue;
                    string s = Format.DeFormat(Format.GetString(buffer, 0, bytesRead), out bool rightFormat);
                    if (!rightFormat) continue;
                    var data = Format.Split(s, Protocol.Separator);
                    foreach (var d in data)
                    {
                        if(OnRecvPreProcess(d))continue;
                        ReceiveBuffer.Write(d);
                    }
                }
                catch (Exception)
                {
                    OnReceiveFailed();
                }
            }
        }

        protected async Task ReceiveAsync()
        {
            byte[] buffer = new byte[Protocol.BufferLength];
            while (On)
            {
                try
                {
                    int bytesRead = await Stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) continue;
                    string s = Format.DeFormat(Format.GetString(buffer, 0, bytesRead), out bool rightFormat);
                    if (!rightFormat) continue;
                    var data = Format.Split(s, Protocol.Separator);
                    foreach (var d in data)
                    {
                        if (OnRecvPreProcess(d)) continue;
                        ReceiveBuffer.Write(d);
                    }
                }
                catch (Exception)
                {
                    OnReceiveFailed();
                }
            }
        }


        public override void SendData(string data)
        {
            if (!Initialized)
            {
                Debug.LogError("[W]WrapperTcp未完成初始化");
                return;
            }
            if (Cancelled)
            {
                Debug.LogError("[W]WrapperTcp已被取消");
                return;
            }
            SendBuffer.Write(data);
        }

        public override CircularQueue<string> RefreshRecvBuffer()
        {
            if (!Initialized)
            {
                Debug.LogError("[W]WrapperTcp未完成初始化");
                return null;
            }
            if (Cancelled)
            {
                Debug.LogError("[W]WrapperTcp已被取消");
                return null;
            }
            return ReceiveBuffer;
        }

        public override void RefreshSendBuffer()
        {
            if (!Initialized)
            {
                Debug.LogError("[W]WrapperTcp未完成初始化");
                return;
            }
            if (Cancelled)
            {
                Debug.LogError("[W]WrapperTcp已被取消");
                return;
            }
            if (SendBuffer==null||SendBuffer.Empty()) return;
            
            string data = Format.EnFormat(Format.Combine(SendBuffer, Protocol.Separator));

            byte[] SendData = Format.GetBytes(data);
            try
            {
                Stream.Write(SendData, 0, SendData.Length);
            }
            catch (Exception)
            {
                OnSendFailed();
            }
        }
        public override void ShutDown()
        {
            Cancelled= true;
            Stream?.Close();
            Client?.Close();
        }
        protected override void ReleaseManagedMenory()
        {
            Stream?.Dispose();
            Client?.Dispose();
        }
        protected override void ReleaseUnmanagedMenory()
        {
            base.ReleaseUnmanagedMenory();
            Stream = null;
            Client = null;
        }
    }
}
