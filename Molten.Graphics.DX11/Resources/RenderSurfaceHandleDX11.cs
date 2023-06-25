﻿using System;
using System.Collections.Generic;
using System.Linq;
using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    internal class RenderSurfaceHandleDX11 : ResourceHandleDX11<ID3D11Resource>
    {
        internal RenderSurfaceHandleDX11(GraphicsResource resource) : 
            base(resource)
        {
            RTV = new RTViewDX11(this);
        }

        public override void Dispose()
        {
            RTV.Release();
            base.Dispose();
        }

        internal RTViewDX11 RTV { get; }
    }
}