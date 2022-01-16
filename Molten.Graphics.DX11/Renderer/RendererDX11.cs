﻿using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Molten.Graphics.Dxgi;

namespace Molten.Graphics
{
    public class RendererDX11 : RenderService
    {
        D3D11 _api;
        DisplayManagerDXGI _displayManager;
        ResourceManager _resourceManager;
        ComputeManager _compute;
        HashSet<Texture2DDX11> _clearedSurfaces;
        Dictionary<Type, RenderStepBase> _steps;
        List<RenderStepBase> _stepList;

        ThreadedDictionary<string, SurfaceConfig> _surfacesByKey;
        ThreadedList<SurfaceConfig> _surfaces;
        ThreadedDictionary<MainSurfaceType, SurfaceConfig> _mainSurfaces;
        DepthStencilSurface _depthSurface;

        internal SpriteBatcherDX11 SpriteBatcher;

        internal GraphicsBuffer StaticVertexBuffer;
        internal GraphicsBuffer DynamicVertexBuffer;
        internal StagingBuffer StagingBuffer;

        internal Material StandardMeshMaterial;
        internal Material StandardMeshMaterial_NoNormalMap;

        public RendererDX11()
        {
            _steps = new Dictionary<Type, RenderStepBase>();
            _stepList = new List<RenderStepBase>();
            _surfacesByKey = new ThreadedDictionary<string, SurfaceConfig>();
            _mainSurfaces = new ThreadedDictionary<MainSurfaceType, SurfaceConfig>();
            _surfaces = new ThreadedList<SurfaceConfig>();
        }

        protected override void OnInitializeAdapter(GraphicsSettings settings)
        {
            _displayManager = new DisplayManagerDXGI();
            _displayManager.Initialize(Log, settings);
        }

        protected override void OnInitialize(EngineSettings settings, Logger mainLog)
        {
            base.OnInitialize(settings, mainLog);

            Device = new DeviceDX11(_api, Log, settings.Graphics, _displayManager);
            _resourceManager = new ResourceManager(this);
            _compute = new ComputeManager(this.Device);
            ShaderCompiler = new HlslCompiler(this, Log);
            _clearedSurfaces = new HashSet<Texture2DDX11>();

            uint maxBufferSize = (uint)ByteMath.FromMegabytes(3.5);
            StaticVertexBuffer = new GraphicsBuffer(Device, BufferMode.Default, BindFlag.BindVertexBuffer | BindFlag.BindIndexBuffer, maxBufferSize);
            DynamicVertexBuffer = new GraphicsBuffer(Device, BufferMode.DynamicRing, BindFlag.BindVertexBuffer | BindFlag.BindIndexBuffer, maxBufferSize);

            StagingBuffer = new StagingBuffer(Device, StagingBufferFlags.Write, maxBufferSize);
            SpriteBatcher = new SpriteBatcherDX11(this, 3000);

            InitializeMainSurfaces(BiggestWidth, BiggestHeight);
            LoadDefaultShaders();
        }

        private void LoadDefaultShaders()
        {
            ShaderCompileResult result = ShaderCompiler.CompileEmbedded("Molten.Graphics.Assets.gbuffer.mfx");
            StandardMeshMaterial = result["material", "gbuffer"] as Material;
            StandardMeshMaterial_NoNormalMap = result["material", "gbuffer-sans-nmap"] as Material;
        }

        public void DispatchCompute(IComputeTask task, int x, int y, int z)
        {
            Device.Dispatch(task as ComputeTask, (uint)x, (uint)y, (uint)z);
        }

        internal void InitializeMainSurfaces(int width, int height)
        {
            CreateMainSurface("scene", MainSurfaceType.Scene, width, height, GraphicsFormat.R8G8B8A8_UNorm);
            CreateMainSurface("normals", MainSurfaceType.Normals, width, height, GraphicsFormat.R11G11B10_Float);
            CreateMainSurface("emissive", MainSurfaceType.Emissive, width, height, GraphicsFormat.R8G8B8A8_UNorm);
            CreateMainSurface("composition1", MainSurfaceType.Composition1, width, height, GraphicsFormat.R16G16B16A16_Float);
            CreateMainSurface("composition2", MainSurfaceType.Composition2, width, height, GraphicsFormat.R16G16B16A16_Float);
            CreateMainSurface("lighting", MainSurfaceType.Lighting, width, height, GraphicsFormat.R16G16B16A16_Float);
            _depthSurface = new DepthStencilSurface(this, width, height, DepthFormat.R24G8_Typeless);
        }

        internal SurfaceConfig RegisterSurface(string key, RenderSurface surface, SurfaceSizeMode sizeMode = SurfaceSizeMode.Full)
        {
            key = key.ToLower();
            if (!_surfacesByKey.TryGetValue(key, out SurfaceConfig config))
            {
                config = new SurfaceConfig(surface, sizeMode);
                _surfacesByKey.Add(key, config);
                _surfaces.Add(config);
            }

            return config;
        }

        internal void CreateMainSurface(
            string key,
            MainSurfaceType mainType, 
            int width, 
            int height, 
            GraphicsFormat format, 
            SurfaceSizeMode sizeMode = SurfaceSizeMode.Full)
        {
            Format dxgiFormat = (Format)format;
            RenderSurface surface = new RenderSurface(this, width, height, dxgiFormat);
            SurfaceConfig config = RegisterSurface(key, surface, sizeMode);
            _mainSurfaces[mainType] = config;
        }

        internal T GetSurface<T>(MainSurfaceType type) where T: RenderSurface
        {
            return _mainSurfaces[type].Surface as T;
        }

        internal T GetSurface<T>(string key) where T : RenderSurface
        {
            return _surfacesByKey[key].Surface as T;
        }

        internal DepthStencilSurface GetDepthSurface()
        {
            return _depthSurface;
        }

        internal T GetRenderStep<T>() where T : RenderStepBase, new()
        {
            Type t = typeof(T);
            RenderStepBase step;
            if (!_steps.TryGetValue(t, out step))
            {
                step = new T();
                step.Initialize(this);
                _steps.Add(t, step);
                _stepList.Add(step);
            }

            return step as T;
        }

        protected override SceneRenderData OnCreateRenderData()
        {
            return new SceneRenderData<Renderable>();
        }

        protected override IRenderChain GetRenderChain()
        {
            return new RenderChain(this);
        }

        protected override void OnPrePresent(Timing time)
        {
            Device.DisposeMarkedObjects();
        }

        protected override void OnPreRenderScene(SceneRenderData sceneData, Timing time)
        {
            
        }

        protected override void OnPostRenderScene(SceneRenderData sceneData, Timing time)
        {
            // Clear the list of used surfaces, ready for the next frame.
            _clearedSurfaces.Clear();
        }

        protected override void OnPreRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            Device.Profiler = camera.Profiler;
        }

        protected override void OnPostRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            Device.Profiler = null;
        }

        protected override void OnPostPresent(Timing time)
        {

        }

        protected override void OnRebuildSurfaces(int requiredWidth, int requiredHeight)
        {
            _surfaces.For(0, 1, (index, config) => config.RefreshSize(requiredWidth, requiredHeight));
            _depthSurface.Resize(requiredWidth, requiredHeight);
        }

        internal void RenderSceneLayer(PipeDX11 pipe, LayerRenderData layerData, RenderCamera camera)
        {
            // TODO To start with we're just going to draw ALL objects in the render tree.
            // Sorting and culling will come later
            LayerRenderData<Renderable> layer = layerData as LayerRenderData<Renderable>;

            foreach (KeyValuePair<Renderable, List<ObjectRenderData>> p in layer.Renderables)
            {
                // TODO sort by material and textures
                foreach (ObjectRenderData data in p.Value)
                {
                    // TODO replace below with render prediction to interpolate between the current and target transform.
                    data.RenderTransform = data.TargetTransform;
                    p.Key.Render(pipe, this, camera, data);
                }
            }
        }

        internal bool ClearIfFirstUse(PipeDX11 pipe, RenderSurface surface, Color color)
        {
            if (!_clearedSurfaces.Contains(surface))
            {
                surface.Clear(pipe, color);
                _clearedSurfaces.Add(surface);
                return true;
            }

            return false;
        }

        internal bool ClearIfFirstUse(PipeDX11 pipe, DepthStencilSurface surface,
            DepthClearFlags flags = DepthClearFlags.Depth, 
            float depth = 1.0f, byte stencil = 0)
        {
            if (!_clearedSurfaces.Contains(surface))
            {
                surface.Clear(pipe, flags, depth, stencil);
                _clearedSurfaces.Add(surface);
                return true;
            }

            return false;
        }

        protected override void OnDisposeBeforeRender()
        {
            for (int i = 0; i < _stepList.Count; i++)
                _stepList[i].Dispose();

            _surfaces.For(0, 1, (index, config) => config.Surface.Dispose());
            _surfaces.Clear();
            _depthSurface.Dispose();
            _mainSurfaces.Clear();
            _surfacesByKey.Clear();

            _resourceManager.Dispose();
            _displayManager?.Dispose();
            SpriteBatcher.Dispose();

            StaticVertexBuffer.Dispose();
            DynamicVertexBuffer.Dispose();
            Device?.Dispose();            
        }

        /// <summary>
        /// Gets the name of the renderer.
        /// </summary>
        public override string Name => "DirectX 11";

        /// <summary>
        /// Gets the display manager bound to the renderer.
        /// </summary>
        public override IDisplayManager DisplayManager => _displayManager;

        internal DeviceDX11 Device { get; private set; }

        public override IComputeManager Compute => _compute;

        internal HlslCompiler ShaderCompiler { get; private set; }

        /// <summary>
        /// Gets the resource manager bound to the renderer.
        /// This is responsible for creating and destroying graphics resources, such as buffers, textures and surfaces.
        /// </summary>
        public override IResourceManager Resources => _resourceManager;
    }
}
