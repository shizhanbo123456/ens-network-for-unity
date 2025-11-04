using System.Net;

namespace ProtocolWrapper.Protocols.Tcp
{
    internal class ModuleTcp
    {
        public static ProtocolBase GetProtocolBase(string ip, int port)
        {
            var tcp = new ClientTcp();
            tcp.Init(ip, port);
            return tcp;
        }
        public static ListenerBase GetListener(IPAddress ip, int port)
        {
            var tcp = new ListenerTcp(ip, port);
            return tcp;
        }
    }
}