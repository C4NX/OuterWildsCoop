using System;

namespace OuterWildsServerLib.Network.Packets
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class NetPacketIdAttribute : Attribute
    {
        public readonly uint Id;

        public NetPacketIdAttribute(uint regId)
        {
            Id = regId;
        }
    }
}
