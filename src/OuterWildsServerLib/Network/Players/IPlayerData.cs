using OuterWildsServerLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Network.Players
{
    /// <summary>
    /// An interface that reflects the player's data on the server.
    /// </summary>
    public interface IPlayerData
    {
        /// <summary>
        /// Get or Update the player's position of this <see cref="IPlayerData"/>
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// Get or Update the player's username of this <see cref="IPlayerData"/>
        /// </summary>
        string Username { get; set; }
    }
}
