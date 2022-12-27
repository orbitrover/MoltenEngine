﻿namespace Molten.Graphics
{
    /// <summary>
    /// An a base class implementation of key shader components (e.g. name, render states, samplers, etc).
    /// </summary>
    public abstract class HlslFoundation : ContextBindable, IShaderElement
    {

        /// <summary>
        /// The texture samplers to be used with the shader/component.
        /// </summary>
        internal ShaderStateBank<ShaderSampler>[] Samplers;

        /// <summary>
        /// The available rasterizer state.
        /// </summary>
        internal ShaderStateBank<GraphicsRasterizerState> RasterizerState = new ShaderStateBank<GraphicsRasterizerState>();

        /// <summary>
        /// The available blend state.
        /// </summary>
        internal ShaderStateBank<GraphicsBlendState> BlendState = new ShaderStateBank<GraphicsBlendState>();

        /// <summary>
        ///The available depth state.
        /// </summary>
        internal ShaderStateBank<GraphicsDepthState> DepthState = new ShaderStateBank<GraphicsDepthState>();

        internal HlslFoundation(DeviceDX11 device) : base(device, ContextBindTypeFlags.Input)
        {
            Samplers = new ShaderStateBank<ShaderSampler>[0];
            Parent = this;
        }

        protected override sealed void OnApply(DeviceContext pipe) { }

        /// <summary>
        /// Gets or sets the number of iterations the shader/component should be run.
        /// </summary>
        public int Iterations { get; set; } = 1;

        public HlslFoundation Parent { get; internal set; }
    }
}
