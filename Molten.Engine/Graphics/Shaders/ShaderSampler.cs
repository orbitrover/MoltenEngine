﻿namespace Molten.Graphics
{
    public abstract class ShaderSampler : GraphicsObject
    {
        protected ShaderSampler(GraphicsDevice device, ref ShaderSamplerParameters parameters) : 
            base(device)
        {
            IsComparisonSampler = parameters.IsComparisonSampler;
        }

        protected override void OnApply(GraphicsQueue queue) { }

        /// <summary>Gets whether or not the sampler a comparison sampler. This is determined by the <see cref="Filter"/> mode.</summary>
        public bool IsComparisonSampler { get; private set; }
    }
}
