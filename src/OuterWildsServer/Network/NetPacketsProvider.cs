using Lidgren.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServer.Network
{
    public class NetPacketsProvider : IEnumerable<KeyValuePair<uint, Type>>
    {
        private SortedList<uint, Type> m_packetsType;
        private NetPeer m_netPeer;

        public NetPacketsProvider(NetPeer peer) { 
            m_packetsType = new SortedList<uint, Type>();
            m_netPeer = peer;
        }

        public NetPacketsProvider AddPacket<T>() where T : INetPacket
        {
            m_packetsType.Add((uint)m_packetsType.Count+1, typeof(T));
            return this;
        }

        public NetPacketsProvider AddPacket<T>(int packetId) where T : INetPacket
        {
            m_packetsType.Add((uint)packetId, typeof(T));
            return this;
        }

        public NetOutgoingMessage Deserialize(INetPacket netPacket)
        {
            var packetIndex = m_packetsType.IndexOfValue(netPacket.GetType());
            if (packetIndex == -1)
                throw new ArgumentException($"This packet {netPacket.GetType().FullName}, is not registered in this packet provider");
            var outgoingMessage = m_netPeer.CreateMessage();
            outgoingMessage.Write(m_packetsType.Keys[packetIndex]);
            netPacket.Deserialize(outgoingMessage);
            return outgoingMessage;
        }

        public IEnumerator<KeyValuePair<uint, Type>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<uint, Type>>)m_packetsType).GetEnumerator();
        }

        public INetPacket Serialize(NetIncomingMessage netIncomingMessage, out uint receivedPacketId)
        {
            uint packerId = 0;

            if (!netIncomingMessage.ReadUInt32(out packerId) || !m_packetsType.ContainsKey(packerId))
            {
                receivedPacketId = packerId;
                return null;
            }

            if (!m_packetsType.ContainsKey(packerId))
                throw new InvalidOperationException($"Not a valid packet, id({packerId}) not found");

            var netPacket = (INetPacket)Activator.CreateInstance(m_packetsType[packerId], new object[0]);
            netPacket.Serialize(netIncomingMessage);

            receivedPacketId = packerId;
            return netPacket;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_packetsType).GetEnumerator();
        }
    }

    public interface INetPacket
    {
        void Serialize(NetIncomingMessage incomingMessage);
        void Deserialize(NetOutgoingMessage netOutgoingMessage);
    }
}
