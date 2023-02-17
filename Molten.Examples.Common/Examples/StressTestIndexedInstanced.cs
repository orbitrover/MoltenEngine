﻿using Molten.Graphics;

namespace Molten.Examples
{
    [Example("Stress Test - Indexed-Instanced", "A stress test which spawns a large number of rotating cubes, which use an indexed mesh with hardware instancing")]
    public class StressTestIndexedInstanced : MoltenExample
    {
        const int CUBE_COUNT = 55000;

        ContentLoadHandle _hMaterial;
        List<SceneObject> _objects;

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            // We need to see further for this test!
            SceneCamera.MaxDrawDistance = 3000f;

            _objects = new List<SceneObject>();
            for (int i = 0; i < CUBE_COUNT; i++)
                SpawnRandomTestCube(TestMesh, 200);
        }

        protected override Mesh GetTestCubeMesh()
        {
            uint maxInstances = CUBE_COUNT + 50;
            IndexedInstancedMesh<VertexColor, InstanceData> cube = Engine.Renderer.Resources.CreateInstancedIndexedMesh<VertexColor, InstanceData>(24, 36, maxInstances);
            cube.SetVertices(SampleVertexData.IndexedColorCubeVertices);
            cube.SetIndices(SampleVertexData.CubeIndices);
            return cube;
        }

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            base.OnLoadContent(loader);
            _hMaterial = loader.Load<Material>("assets/BasicColorInstanced.mfx");

            loader.OnCompleted += Loader_OnCompleted;
        }
        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Close();
                return;
            }

            TestMesh.Material = _hMaterial.Get<Material>();
        }

        private void SpawnRandomTestCube(Mesh mesh, int spawnRadius)
        {
            int maxRange = spawnRadius * 2;
            SceneObject obj = MainScene.CreateObject(new Vector3F()
            {
                X = -spawnRadius + (float)(Rng.NextDouble() * maxRange),
                Y = -spawnRadius + (float)(Rng.NextDouble() * maxRange),
                Z = spawnRadius + (float)(Rng.NextDouble() * maxRange)
            });
            RenderableComponent meshCom = obj.Components.Add<RenderableComponent>();
            meshCom.RenderedObject = mesh;

            _objects.Add(obj);
        }

        protected override void OnUpdate(Timing time)
        {
            var rotateAngle = 1.2f * time.Delta;
            for(int i = _objects.Count-1; i >= 0; i--)
            {
                SceneObject obj = _objects[i];
                obj.Transform.LocalRotationX += rotateAngle + (0.0001f * i);
                obj.Transform.LocalRotationY += rotateAngle + (0.0001f * i);
                obj.Transform.LocalRotationZ += rotateAngle * 0.7f * time.Delta;
            }

            base.OnUpdate(time);
        }
    }
}
