﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    public abstract class ResourceVK : GraphicsResource
    {
        public ResourceVK(GraphicsDevice device, GraphicsBindTypeFlags bindFlags) : 
            base(device, bindFlags)
        {

        }

        public override unsafe void* Handle => throw new NotImplementedException();

        internal unsafe abstract DeviceMemory* Memory { get; }
    }
}
