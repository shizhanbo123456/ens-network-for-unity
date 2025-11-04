using System;
using UnityEngine;

namespace ProtocolWrapper
{
    internal abstract class ProtocolBase : Disposable
    {
        public string IP;
        public int Port;
        protected CircularQueue<string> SendBuffer;
        protected CircularQueue<string> ReceiveBuffer;

        public bool Initialized=false;
        public bool Cancelled = false;
        public bool On
        {
            get
            {
                return Initialized && !Cancelled;
            }
        }

        public int Id;

        protected void Init(string ip, int port)
        {
            SendBuffer = new CircularQueue<string>();
            ReceiveBuffer = new CircularQueue<string>();
            IP = ip;
            Port = port;

            Id = Protocol.id++;

            lock(Protocol.basesLock)
                Protocol.ProtocolBases.Add(this);
        }

        //均为非阻塞，线程安全的
        public abstract void SendData(string data);
        public abstract void RefreshSendBuffer();
        public abstract CircularQueue<string> RefreshRecvBuffer();
        public abstract void ShutDown();

        protected virtual void OnReceiveFailed()
        {
            Protocol.OnServerReceiveFailed.SetTrigger();
        }
        protected virtual void OnSendFailed()
        {
            Protocol.OnServerSendFailed.SetTrigger();
        }
        /// <summary>
        /// 返回值=ignore?
        /// </summary>
        public abstract bool OnRecvPreProcess(string data);

        protected override void ReleaseUnmanagedMenory()
        {
            SendBuffer=null; 
            ReceiveBuffer=null;
        }
    }
}
