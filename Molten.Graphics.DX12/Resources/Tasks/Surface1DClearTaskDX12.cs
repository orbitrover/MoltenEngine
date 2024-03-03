﻿namespace Molten.Graphics.DX12;

internal class Surface1DClearTaskDX12 : GraphicsResourceTask<RenderSurface1DDX12>
{
    public Color Color;

    public override void ClearForPool()
    {
        Color = Color.Black;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GraphicsQueue queue)
    {
        Resource.Apply(queue);
        (queue as GraphicsQueueDX12).Clear(Resource, Color);
        return true;
    }
}