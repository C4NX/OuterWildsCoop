using Lidgren.Network;
using OuterWildsServerLib.Network.Players;
using OuterWildsServerLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Network.Players
{
    public class OWPlayer : IPlayerData
    {
        private NetConnection _connection;
        private Guid _id;
        private string _username;

        private Vector3 position;

        public Vector3 Position { get => position; set => position = value; } //TODO: Update position to client.
        public string Username { get => _username; set => _username = value; } //TODO: Update username to client.

        public OWPlayer(Guid id, NetConnection netConnection, string username) 
        {
            if(username == null)
                throw new ArgumentNullException("username");

            _connection = netConnection;
            _id = id;
            _username = username;
            position = Vector3.Zero;
        }

        public Guid GetGuid() => _id;

        public string GetUsername() => _username;

        public NetConnection GetConnection() => _connection;

        public override string ToString() => $"{_id} ({_username})";


        public static OWPlayer CreateFake(Guid guid, string username) => new OWPlayer(guid, null, username);
    }
}
