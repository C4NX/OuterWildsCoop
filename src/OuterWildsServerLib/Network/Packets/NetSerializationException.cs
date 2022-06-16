using System;

namespace OuterWildsServerLib.Network.Packets
{
    /// <summary>
    /// An exception that is thrown when a serialization error occurs in a <see cref="NetPacketProvider"/>
    /// </summary>
    [Serializable]
    public class NetSerializationException : Exception
    {
        public NetSerializationException() { }
        public NetSerializationException(string message) : base(message) { }
        public NetSerializationException(string message, Exception inner) : base(message, inner) { }
        protected NetSerializationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
