﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Networking
{
    public class NetworkMessage : INetworkMessage
    {
        MoltenNetworkService _networkService;

        public byte[] Data { get; internal set; }
        public int Sequence { get; internal set; }

        internal NetworkMessage(MoltenNetworkService service, byte[] data, int sequence)
        {
            _networkService = service;
            Data = data;
            Sequence = sequence;
        }

        internal void Update(byte[] data, int sequence)
        {
            Data = data;
            Sequence = sequence;
        }

        internal void Clear()
        {
            Data = null;
            Sequence = 0;
        }

        public void Recycle()
        {
            _networkService.RecycleMessage(this);
        }
    }
}
