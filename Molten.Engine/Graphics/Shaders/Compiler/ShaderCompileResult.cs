﻿namespace Molten.Graphics
{
    public sealed class ShaderCompileResult : EngineObject
    {
        internal List<HlslShader> Shaders { get; } = new List<HlslShader>();

        protected override void OnDispose() { }

        /// <summary>
        /// Gets a <see cref="HlslGraphicsObject"/> of the specified name which was built successfully.
        /// </summary>
        /// <param name="shaderName">The name of the shader given to it it via its XML definition.</param>
        /// <returns></returns>
        public HlslShader this[string shaderName]
        {
            get
            {
                foreach (HlslShader shader in Shaders)
                {
                    if (shader.Name == shaderName)
                        return shader;
                }

                return null;
            }
        }
    }
}
