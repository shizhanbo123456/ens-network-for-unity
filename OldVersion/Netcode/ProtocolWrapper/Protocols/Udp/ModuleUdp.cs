using System.Net;

namespace ProtocolWrapper.Protocols.Udp
{
    internal class ModuleUdp
    {
        public static ProtocolBase GetProtocolBase(string ip, int port)
        {
            var udp = new ClientUdp();
            udp.Init(ip, port);
            return udp;
        }
        public static ListenerBase GetListener(IPAddress ip, int port)
        {
            var udp = new ListenerUdp(ip, port);
            return udp;
        }
    }
}