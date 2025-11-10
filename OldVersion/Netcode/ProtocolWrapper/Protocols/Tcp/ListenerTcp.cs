using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ProtocolWrapper.Protocols.Tcp
{
    /// <summary>
    /// 调用StartListening后立即开始侦听，需要调用Update<br></br>
    /// 调用ShutDown后Cancelled设置为true之后回收
    /// </summary>
    internal class ListenerTcp : ListenerBase
    {
        private bool nullEvent=true;
        public List<ConnectionTcp> Connections=new List<ConnectionTcp>();
        private TcpListener Listener;

        public ListenerTcp(IPAddress ip,int port) : base(ip, port)
        {
            Listener = new TcpListener(IPAddress.Any, Port);
            Listening = false;
        }
        public override void StartListening()
        {
            if (Listening) throw new Exception("[W]Listener已经启动");
            Listener.Start();
            Listening = true;
            if (Protocol.mode == Mode.Multithreading)
            {
                Thread AcceptClientsThread = new Thread(new ThreadStart(AcceptClients));
                AcceptClientsThread.Start();
            }
            else
            {
                _ = AcceptClientsAsync();
            }
        }
        public override void EndListening()
        {
            if (!Listening) throw new Exception("[W]Listener已经关闭");
            Listening = false;
            Listener.Stop();
        }
        protected virtual void AcceptClients()
        {
            try
            {
                while (Listening)
                {
                    TcpClient Client = Listener.AcceptTcpClient();//------------------------------会导致线程阻塞
                    if (!Listening)
                    {
                        Client.Close();
                        Client.Dispose();
                        continue;
                    }
                    var Connection = new ConnectionTcp();
                    if (Protocol.OnRecvConnection != null)
                    {
                        Connection.Init(Client, this);
                        Protocol.OnRecvConnection.Invoke(Connection, ++connectionIndex);
                    }
                    Connections.Add(Connection);
                    if (Protocol.DevelopmentDebug) Debug.Log("[W]有新客户端连接");
                }
            }
            catch (SocketException)
            {
                if (Protocol.DevelopmentDebug) Debug.Log("[W]服务器停止侦听客户端");
            }
            catch (Exception ex)
            {
                if (Protocol.DevelopmentDebug) Debug.LogWarning($"[W]服务器停止侦听客户端：{ex.Message}");
            }
        }
        private async Task AcceptClientsAsync()
        {
            try
            {
                while (Listening)
                {
                    TcpClient Client = await Listener.AcceptTcpClientAsync(); // 异步接受客户端连接  
                    if (!Listening)
                    {
                        Client.Close();
                        Client.Dispose();
                        continue;
                    }
                    var Connection = new ConnectionTcp();
                    if (Protocol.OnRecvConnection != null)
                    {
                        Connection.Init(Client, this);
                        Protocol.OnRecvConnection.Invoke(Connection, ++connectionIndex);
                    }
                    Connections.Add(Connection);
                    if (Protocol.DevelopmentDebug) Debug.Log("[W]有新客户端连接");
                }
            }
            catch (Exception ex)
            {
                if (Protocol.DevelopmentDebug) Debug.LogWarning($"[W]服务器停止侦听客户端：{ex.Message}");
            }
        }
        public override void Update()
        {
            for(int i= Connections.Count - 1; i >= 0; i--)
            {
                if (Connections[i].Cancelled)
                {
                    Connections[i].Dispose();
                    Connections.RemoveAt(i);
                }
            }
        }
        public override void ShutDown()
        {
            base.ShutDown();
            foreach (var c in Connections) c.ShutDown();
        }


        public override void Broadcast(string data)
        {
            if (Check()) throw new Exception("[W]已赋予事件，此层连接集合已弃用");
            foreach (var i in Connections) i.SendData(data);
        }
        public override void Broadcast(string data, int ignore)
        {
            if (Check()) throw new Exception("[W]已赋予事件，此层连接集合已弃用");
            foreach (var i in Connections) if (i.Id!=ignore) i.SendData(data);
        }
        public override void PTP(string data, List<int> targets)
        {
            if (Check()) throw new Exception("[W]已赋予事件，此层连接集合已弃用");
            foreach (var i in Connections) if (targets.Contains(i.Id)) i.SendData(data);
        }
        public override void PTP(string data, int target)
        {
            if (Check()) throw new Exception("[W]已赋予事件，此层连接集合已弃用");
            foreach (var i in Connections) if (target == i.Id) { i.SendData(data); return; }
        }
        private bool Check()//返回是否有事件
        {
            if (!nullEvent) return true;
            if (Protocol.OnRecvConnection != null)
            {
                nullEvent= false;
                return true;
            }
            return false;
        }


        protected override void ReleaseManagedMenory()
        {
            foreach (var c in Connections) c.Dispose();
            Connections.Clear();
        }
        protected override void ReleaseUnmanagedMenory()
        {
            Listener = null;
            Connections = null;
        }
    }
}
