using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace ProtocolWrapper.Protocols.Udp
{
    internal class ConnectionUdp : WrapperUdp
    {
        private IPEndPoint EP;
        private ListenerUdp Listener;
        public void Init(ListenerUdp listener,IPEndPoint ep)
        {
            EP = ep;
            Listener = listener;
            Init(listener.client,ep.Address,ep.Port);
            Initialized= true;
        }
        public override void ShutDown()
        {
            base.ShutDown();
            Listener.Connections.Remove(EP);
        }
        public override bool OnRecvPreProcess(string data)
        {
            if (Protocol.ConnectionRecvImmediateInvoke != null)
            {
                return Protocol.ConnectionRecvImmediateInvoke.Invoke(data);
            }
            return false;
        }
        protected override void ReleaseUnmanagedMenory()
        {
            Listener = null;
            EP= null;
            base.ReleaseUnmanagedMenory();
        }
    }
}