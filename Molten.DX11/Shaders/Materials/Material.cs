﻿using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class Material : HlslShader, IMaterial
    {
        internal MaterialPass[] Passes = new MaterialPass[0];
        internal unsafe IDxcBlob* InputStructureByteCode;

        // TODO move these to back to the HLSL compiler via a metadata system.
        internal string DefaultVSEntryPoint;
        internal string DefaultGSEntryPoint;
        internal string DefaultDSEntryPoint;
        internal string DefaultHSEntryPoint;
        internal string DefaultPSEntryPoint;

        Dictionary<string, MaterialPass> _passesByName;

        public int PassCount => Passes.Length;

        internal Material(DeviceDX11 device, string filename) : base(device, filename)
        {
            _passesByName = new Dictionary<string, MaterialPass>();
        }

        /// <summary>
        /// Creates a new pass using the default shader stage entry points.
        /// </summary>
        internal void AddDefaultPass()
        {
            // TODO move this back to the HLSL compiler via a metadata system.
            MaterialPass defaultPass = new MaterialPass(this, "<Auto-generated pass>");
            defaultPass.VertexShader.EntryPoint = DefaultVSEntryPoint;
            defaultPass.GeometryShader.EntryPoint = DefaultGSEntryPoint;
            defaultPass.DomainShader.EntryPoint = DefaultDSEntryPoint;
            defaultPass.HullShader.EntryPoint = DefaultHSEntryPoint;
            defaultPass.PixelShader.EntryPoint = DefaultPSEntryPoint;
            AddPass(defaultPass);
        }

        internal void AddPass(MaterialPass pass)
        {
            int id = 0;
            if (Passes == null)
            {
                Passes = new MaterialPass[1];
            }
            else
            {
                id = Passes.Length;
                Array.Resize(ref Passes, Passes.Length + 1);
            }

            Passes[id] = pass;
        }

        public IMaterialPass GetPass(uint index)
        {
            return Passes[index];
        }

        public IMaterialPass GetPass(string name)
        {
            return _passesByName[name];
        }

        internal override void PipelineDispose()
        {
            for (int i = 0; i < Passes.Length; i++)
                Passes[i].Dispose();
        }

        internal ObjectMaterialProperties Object { get; set; }

        internal LightMaterialProperties Light { get; set; }

        internal SceneMaterialProperties Scene { get; set; }

        internal GBufferTextureProperties Textures { get; set; }

        internal SpriteBatchMaterialProperties SpriteBatch { get; set; }
    }
}
