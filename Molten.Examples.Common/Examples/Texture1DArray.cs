﻿using Molten.Graphics;

namespace Molten.Examples
{
    [Example("Texture Arrays - 1D", "Demonstrates how 1D texture arrays are used")]
    public class Texture1DArray : MoltenExample
    {
        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture;

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            base.OnLoadContent(loader);

            _hMaterial = loader.Load<Material>("assets/BasicTexture1D.mfx");
            _hTexture = loader.Load<ITexture>("assets/1d_1.png");
            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Close();
                return;
            }

            Material mat = _hMaterial.Get<Material>();
            ITexture texture = _hTexture.Get<ITexture>();

            mat.SetDefaultResource(texture, 0);
            TestMesh.Material = mat;
        }

        protected override Mesh GetTestCubeMesh()
        {
            Mesh<CubeArrayVertex> cube = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
            cube.SetVertices(SampleVertexData.TextureArrayCubeVertices);
            return cube;
        }
    }
}
