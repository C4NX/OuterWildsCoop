using OuterWildsServerLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Network.Players
{
    /// <summary>
    /// Type that acts as a basic container for the player data.
    /// </summary>
    public class PlayerDataFields : IPlayerData
    {
        public Vector3 Position { get; set; }
        public string Username { get; set; }
    }
}
