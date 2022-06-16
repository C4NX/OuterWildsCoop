using Lidgren.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Network.Packets
{
    /// <summary>
    /// A packet management and serialization class.
    /// Can be enumerated as <see cref="KeyValuePair{uint, Type}"/>
    /// </summary>
    public class NetPacketProvider : IEnumerable<KeyValuePair<uint, Type>>
    {
        private SortedList<uint, Type> m_packetsType;
        private NetPeer m_netPeer;

        /// <summary>
        /// Create a <see cref="NetPacketProvider"/> instance for a <see cref="NetPeer"/>
        /// </summary>
        /// <param name="peer"></param>
        public NetPacketProvider(NetPeer peer) { 
            m_packetsType = new SortedList<uint, Type>();
            m_netPeer = peer;
        }

        /// <summary>
        /// Register a type of <see cref="INetPacket"/>, and automatically add the last available id.
        /// </summary>
        /// <typeparam name="T">The <see cref="INetPacket"/> type to register</typeparam>
        /// <returns>The current instance of the <see cref="NetPacketProvider"/></returns>
        public NetPacketProvider AddPacket<T>() where T : INetPacket
        {
            m_packetsType.Add((uint)m_packetsType.Count+1, typeof(T));
            return this;
        }

        /// <summary>
        /// Register a type of <see cref="INetPacket"/>, and use a manually defined id.
        /// </summary>
        /// <param name="packetId">id used in this <see cref="NetPacketProvider"/></param>
        /// <typeparam name="T">The <see cref="INetPacket"/> type to register</typeparam>
        /// <returns>The current instance of the <see cref="NetPacketProvider"/></returns>
        public NetPacketProvider AddPacket<T>(uint packetId) where T : INetPacket
        {
            m_packetsType.Add(packetId, typeof(T));
            return this;
        }

        /// <summary>
        /// Register a type of <see cref="INetPacket"/>, and use a manually defined id.
        /// </summary>
        /// <param name="netPacketType">The type you want to register</param>
        /// <param name="packetId">id used in this <see cref="NetPacketProvider"/></param>
        /// <returns>he current instance of the <see cref="NetPacketProvider"/></returns>
        /// <exception cref="ArgumentException">If the type is not assignable from an inetpacket or is just the interface itself</exception>
        public NetPacketProvider AddPacket(Type netPacketType, uint packetId)
        {
            if (!typeof(INetPacket).IsAssignableFrom(netPacketType) || netPacketType == typeof(INetPacket))
                throw new ArgumentException("netPacketType is not a INetPacket");

            m_packetsType.Add(packetId, netPacketType);
            return this;
        }

        /// <summary>
        /// Add all available public packets in an assembly. 
        /// </summary>
        /// <param name="assembly">The assembly to use</param>
        public void AddPackets(Assembly assembly)
        {
            foreach (var item in assembly.GetExportedTypes())
            {
                if (typeof(INetPacket).IsAssignableFrom(item) && item != typeof(INetPacket))
                {
                    var netPacketId = item.GetCustomAttribute<NetPacketIdAttribute>();
                    if (netPacketId != null)
                        AddPacket(item, netPacketId.Id);
                    else
                        AddPacket(item, (uint)m_packetsType.Count + 1);
                }
            }
        }

        /// <summary>
        /// Serializes an <see cref="INetPacket"/> into a <see cref="NetOutgoingMessage"/> ready to be sent.
        /// </summary>
        /// <param name="netPacket">The <see cref="INetPacket"/> to serialize.</param>
        /// <returns>The <see cref="NetOutgoingMessage"/> seriliazed</returns>
        /// <exception cref="ArgumentException">If the <see cref="INetPacket"/> is not registered in this <see cref="NetPacketProvider"/></exception>
        /// <exception cref="NetSerializationException">If an error occurs during the serialization.</exception>
        public NetOutgoingMessage Serialize(INetPacket netPacket)
        {
            var packetIndex = m_packetsType.IndexOfValue(netPacket.GetType());
            if (packetIndex == -1)
                throw new ArgumentException($"This packet {netPacket.GetType().FullName}, is not registered in this packet provider");
            var outgoingMessage = m_netPeer.CreateMessage();
            outgoingMessage.Write(m_packetsType.Keys[packetIndex]);

            try
            {
                netPacket.Serialize(outgoingMessage);
            }
            catch (Exception ex)
            {
                throw new NetSerializationException($"An error occurred while serializing a packet of type {netPacket.GetType().FullName} ({packetIndex})", ex);
            }

            return outgoingMessage;
        }


        /// <summary>
        /// Deserialize a <see cref="NetIncomingMessage"/> and return the packet id and the <see cref="INetPacket"/> if no error was thrown during deserialization. 
        /// </summary>
        /// <param name="netIncomingMessage"></param>
        /// <param name="receivedPacketId"></param>
        /// <returns>The <see cref="INetPacket"/> deserialized.</returns>
        /// <exception cref="ArgumentException">If the packet does not have a valid id.</exception>
        /// <exception cref="NetPacketProvider">If an error occurs during the deserialization.</exception>
        public INetPacket Deserialize(NetIncomingMessage netIncomingMessage, out uint receivedPacketId)
        {
            uint packetId = 0;

            if (!netIncomingMessage.ReadUInt32(out packetId) || !m_packetsType.ContainsKey(packetId))
            {
                receivedPacketId = packetId;
                return null;
            }

            if (!m_packetsType.ContainsKey(packetId))
                throw new ArgumentException($"Not a valid packet, id({packetId}) not found");


            INetPacket netPacket;

            try
            {
                netPacket = (INetPacket)Activator.CreateInstance(m_packetsType[packetId], new object[0]);
                netPacket.Deserialize(netIncomingMessage);
            }
            catch (Exception ex)
            {
                throw new NetSerializationException($"An error occurred while serializing a packet of type {m_packetsType[packetId].FullName} ({packetId})", ex);
            }

            receivedPacketId = packetId;
            return netPacket;
        }

        public IEnumerator<KeyValuePair<uint, Type>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<uint, Type>>)m_packetsType).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_packetsType).GetEnumerator();
        }
    }

    /// <summary>
    /// Represent a simple network packet that can either be serialized, or deserialized. can be registered to a <see cref="NetPacketProvider"/>.
    /// </summary>
    public interface INetPacket
    {
        /// <summary>
        /// Deserialize a <see cref="NetIncomingMessage"/> into this <see cref="INetPacket"/>.
        /// </summary>
        /// <param name="incomingMessage"></param>
        void Deserialize(NetIncomingMessage incomingMessage);

        /// <summary>
        /// Serializes this <see cref="INetPacket"/> into a <see cref="NetOutgoingMessage"/> ready to be sent.
        /// </summary>
        void Serialize(NetOutgoingMessage netOutgoingMessage);
    }
}
