using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using OuterWildsServerLib.Utils;

namespace OuterWildsServerLib.Network
{
    public static class LidgrenUtils
    {
        public static void Write(this NetBuffer netBuffer, Guid guid) => netBuffer.Write(guid.ToByteArray());
        public static Guid ReadGuid(this NetBuffer netBuffer) => new Guid(netBuffer.ReadBytes(16));

        public static void Write(this NetBuffer netBuffer, Vector3 vector3)
        {
            netBuffer.Write(vector3.X);
            netBuffer.Write(vector3.Y);
            netBuffer.Write(vector3.Z);
        }
        public static Vector3 ReadVector3(this NetBuffer netBuffer)
            => new Vector3(netBuffer.ReadFloat(), netBuffer.ReadFloat(), netBuffer.ReadFloat());
    }
}
