﻿using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildsCoop.Network.Players
{
    public class OwPlayer
    {
        private NetConnection _connection;
        private Guid _id;
        private string _username;

        public OwPlayer(Guid id, NetConnection netConnection, string username) 
        {
            if(username == null)
                throw new ArgumentNullException("username");

            _connection = netConnection;
            _id = id;
            _username = username;
        }

        public Guid GetGuid() => _id;

        public string GetUsername() => _username;
    }
}