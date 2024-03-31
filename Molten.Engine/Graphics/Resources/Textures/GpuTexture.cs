﻿using Molten.Graphics.Textures;
using System.Reflection.Emit;

namespace Molten.Graphics;

/// <summary>
/// A delegate for texture event handlers.
/// </summary>
/// <param name="texture">The texture instance that triggered the event.</param>
public delegate void TextureHandler(GpuTexture texture);

public abstract class GpuTexture : GpuResource, ITexture
{
    /// <summary>
    /// Invoked after resizing of the texture has completed.
    /// </summary>
    public event TextureHandler OnResize;

    TextureDimensions _dimensions;
    GpuResourceFormat _format;

    /// <summary>
    /// Creates a new instance of <see cref="GpuTexture"/>.
    /// </summary>
    /// <param name="device">The <see cref="GpuTexture"/> that the buffer is bound to.</param>
    /// <param name="dimensions">The dimensions of the texture.</param>
    /// <param name="format">The <see cref="GpuResourceFormat"/> of the texture.</param>
    /// <param name="flags">Resource flags which define how the texture can be used.</param>
    /// <param name="name">The name of the texture. This is mainly used for debug purposes.</param>
    /// <exception cref="ArgumentException"></exception>
    protected GpuTexture(GpuDevice device, ref TextureDimensions dimensions, GpuResourceFormat format, GpuResourceFlags flags, string name)
        : base(device, flags)
    {
        if(dimensions.IsCubeMap && dimensions.ArraySize % 6 != 0)
            throw new ArgumentException("The array size of a cube map must be a multiple of 6.", nameof(dimensions.ArraySize));

        LastFrameResizedID = device.Renderer.FrameID;
        ValidateFlags();

        MSAASupport msaaSupport = MSAASupport.NotSupported; // TODO re-support. _renderer.Device.Features.GetMSAASupport(format, aaLevel);
        _dimensions = dimensions;

        Name = string.IsNullOrWhiteSpace(name) ? $"{GetType().Name}_{Width}x{Height}" : name;

        MultiSampleLevel = dimensions.MultiSampleLevel > AntiAliasLevel.Invalid ? dimensions.MultiSampleLevel : AntiAliasLevel.None;
        SampleQuality = msaaSupport != MSAASupport.NotSupported ? dimensions.SampleQuality : MSAAQuality.Default;
        ResourceFormat = format;
    }

    protected void InvokeOnResize()
    {
        OnResize?.Invoke(this);
    }

    private void ValidateFlags()
    {
        // Validate RT mip-maps
        if (Flags.Has(GpuResourceFlags.MipMapGeneration))
        {
            if (Flags.Has(GpuResourceFlags.DenyShaderAccess) || !(this is IRenderSurface2D))
                throw new GpuResourceException(this, "Mip-map generation is only available on render-surface shader resources.");
        }

        // Only staging resources have CPU-write access.
        if (Flags.Has(GpuResourceFlags.UploadMemory))
        {
            if (!Flags.Has(GpuResourceFlags.DenyShaderAccess))
                throw new GpuResourceException(this, "Staging textures cannot allow shader access. Add GraphicsResourceFlags.NoShaderAccess flag.");
        }
    }

    internal void ResizeTextureImmediate(GpuCommandList cmd, in TextureDimensions newDimensions, GpuResourceFormat newFormat)
    {
        // Avoid resizing/recreation if nothing has actually changed.
        if (_dimensions == newDimensions && ResourceFormat == newFormat)
            return;

        _dimensions = newDimensions;
        ResourceFormat = newFormat;

        OnResizeTextureImmediate(cmd, in newDimensions, newFormat);
        LastFrameResizedID = Device.Renderer.FrameID;
        Version++;

        OnResize?.Invoke(this);
    }

    /// <summary>
    /// Resizes the current <see cref="GpuTexture"/>.
    /// </summary>
    /// <param name="priority">The priority of the resize operation.</param>
    /// <param name="newWidth">The new width.</param>      
    /// <param name="newMipMapCount">The number of mip-map levels per array slice/layer. If set to 0, the current <see cref="MipMapCount"/> will be used.</param>
    /// <param name="newFormat">The new format. If set to <see cref="GpuResourceFormat.Unknown"/>, the existing format will be used.</param>
    /// <param name="completeCallback">A callback to invoke once the resize operation has been completed.</param>
    public void Resize(GpuPriority priority, uint newWidth, GpuResourceFormat newFormat = GpuResourceFormat.Unknown, uint newMipMapCount = 0,
        GpuTask.EventHandler completeCallback = null)
    {
        Resize(priority, newWidth, Height, newFormat, ArraySize, newMipMapCount, Depth, completeCallback);
    }

    /// <summary>
    /// Resizes the current <see cref="GpuTexture"/>.
    /// </summary>
    /// <param name="priority">The priority of the resize operation.</param>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height. If the texture is 1D, height will be defaulted to 1.</param>
    /// <param name="arraySize">For 3D textures, this is the new depth dimension. 
    /// For every other texture type, this is the number of array slices/layers, or the array size.
    /// <para>If set to 0, the existing <see cref="GpuTexture.Depth"/> or <see cref="GpuTexture.ArraySize"/> will be used.</para></param>
    /// <param name="depth">The new depth. Only applicable for 3D textures.</param>
    /// <param name="mipMapCount">The number of mip-map levels per array slice/layer. If set to 0, the current <see cref="GpuTexture.MipMapCount"/> will be used.</param>
    /// <param name="newFormat">The new format. If set to <see cref="GpuResourceFormat.Unknown"/>, the existing format will be used.</param>
    /// <param name="completeCallback">A callback to invoke once the resize operation has been completed.</param>
    public void Resize(GpuPriority priority, uint width, uint height, GpuResourceFormat newFormat = GpuResourceFormat.Unknown,
        uint arraySize = 0, uint mipMapCount = 0, uint depth = 0, 
        GpuTaskHandler completeCallback = null)
    {
        if (this is ITexture1D)
            height = 1;

        if (this is not ITexture3D)
            depth = 1;

        TextureResizeTask task = Device.Tasks.Get<TextureResizeTask>();
        task.NewFormat = newFormat == GpuResourceFormat.Unknown ? ResourceFormat : newFormat;
        task.Resource = this;
        task.OnCompleted += completeCallback;
        task.NewDimensions = new TextureDimensions()
        {
            Width = width,
            Height = height,
            ArraySize = arraySize > 0 ? arraySize : ArraySize,
            Depth = depth > 0 ? depth : Depth,
            MipMapCount = mipMapCount > 0 ? mipMapCount : MipMapCount
        };

        Device.Tasks.Push(priority, task);
    }

    /// <summary>Copies data fom the provided <see cref="TextureData"/> instance into the current texture.</summary>
    /// <param name="priority">The priority of the operation.</param>
    /// <param name="cmd">The command list used when executing the operation immediately. Can be null if not using <see cref="GpuPriority.Immediate"/>.</param>
    /// <param name="data"></param>
    /// <param name="levelStartIndex">The starting mip-map index within the provided <see cref="TextureData"/>.</param>
    /// <param name="arrayStartIndex">The starting array slice index within the provided <see cref="TextureData"/>.</param>
    /// <param name="levelCount">The number of mip-map levels to copy per array slice, from the provided <see cref="TextureData"/>.</param>
    /// <param name="arrayCount">The number of array slices to copy from the provided <see cref="TextureData"/>.</param>
    /// <param name="destLevelIndex">The mip-map index within the current texture to start copying to.</param>
    /// <param name="destArrayIndex">The array slice index within the current texture to start copying to.</param>
    /// <param name="completeCallback">A callback to invoke once the data has been transferred to the GPU.</param>
    public unsafe void SetData(GpuPriority priority, GpuCommandList cmd, TextureData data, uint levelStartIndex = 0, uint arrayStartIndex = 0,
        uint levelCount = 0, uint arrayCount = 0,
        uint destLevelIndex = 0, uint destArrayIndex = 0,
        GpuTaskHandler completeCallback = null)
    {
        TextureSetDataTask task = new();
        task.Data = data;
        task.Resource = this;
        task.LevelStartIndex = levelStartIndex;
        task.ArrayStartIndex = arrayStartIndex;
        task.LevelCount = levelCount;
        task.ArrayCount = arrayCount;
        task.DestLevelIndex = destLevelIndex;
        task.DestArrayIndex = destArrayIndex;
        task.OnCompleted += completeCallback;

        Device.Tasks.Push(priority, ref task, cmd);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="priority">The priority of the operation.</param>
    /// <param name="cmd"></param>
    /// <param name="data"></param>
    /// <param name="mipIndex"></param>
    /// <param name="arraySlice"></param>
    /// <param name="completeCallback">A callback to invoke once the data has been transferred to the GPU.</param>
    public unsafe void SetSubResourceData(GpuPriority priority, GpuCommandList cmd, TextureSlice data, uint mipIndex, uint arraySlice, GpuTaskHandler completeCallback = null)
    {
        TextureSetSubResourceTask<byte> task = new();
        task.Initialize(data.Data, 1, 0, data.TotalBytes);
        task.Pitch = data.Pitch;
        task.Resource = this;
        task.ArrayIndex = arraySlice;
        task.MipLevel = mipIndex;
        task.OnCompleted += completeCallback;

        Device.Tasks.Push(priority, ref task, cmd);
    }

    public unsafe void SetSubResourceData<T>(GpuPriority priority, uint level, T[] data, uint startIndex, uint count, uint pitch, uint arrayIndex,
        GpuTask.EventHandler completeCallback = null)
        where T : unmanaged
    {
        fixed (T* ptrData = data)
        {
            TextureSetSubResourceTask task = Device.Tasks.Get<TextureSetSubResourceTask>();
            task.Initialize(ptrData, (uint)sizeof(T), startIndex, count);
            task.Pitch = pitch;
            task.ArrayIndex = arrayIndex;
            task.MipLevel = level;
            task.Resource = this;
            task.OnCompleted += completeCallback;
            Device.Tasks.Push(priority, task);
        }
    }

    public unsafe void SetSubResourceData<T>(GpuPriority priority, ResourceRegion area, T[] data, uint bytesPerPixel, uint level, uint arrayIndex = 0,
        GpuTask.EventHandler completeCallback = null)
        where T : unmanaged
    {
        fixed (T* ptrData = data)
            SetSubResourceData(priority, area, ptrData, (uint)data.Length, bytesPerPixel, level, arrayIndex, completeCallback);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">The type of data to be sent to the GPU texture.</typeparam>
    /// <param name="priority">The priority of the operation.</param>
    /// <param name="region"></param>
    /// <param name="data"></param>
    /// <param name="numElements"></param>
    /// <param name="bytesPerPixel"></param>
    /// <param name="level"></param>
    /// <param name="arrayIndex"></param>
    /// <param name="completeCallback">A callback to invoke once the resize operation has been completed.</param>
    /// <exception cref="Exception"></exception>
    public unsafe void SetSubResourceData<T>(GpuPriority priority, ResourceRegion region, T* data,
        uint numElements, uint bytesPerPixel, uint level, uint arrayIndex = 0,
        GpuTask.EventHandler completeCallback = null)
        where T : unmanaged
    {
        uint texturePitch = region.Width * bytesPerPixel;
        uint pixels = region.Width * region.Height;
        uint expectedBytes = pixels * bytesPerPixel;
        uint dataBytes = (uint)(numElements * sizeof(T));

        if (pixels != numElements)
            throw new Exception($"The provided data does not match the provided area of {region.Width}x{region.Height}. Expected {expectedBytes} bytes. {dataBytes} bytes were provided.");

        // Do a bounds check
        ResourceRegion texBounds = new ResourceRegion(0, 0, 0, Width, Height, Depth);
        if (!texBounds.Contains(region))
            throw new Exception("The provided area would go outside of the current texture's bounds.");

        TextureSetSubResourceTask task = Device.Tasks.Get<TextureSetSubResourceTask>();
        task.Initialize(data, (uint)sizeof(T), 0, numElements);
        task.Resource = this;
        task.Pitch = texturePitch;
        task.StartIndex = 0;
        task.ArrayIndex = arrayIndex;
        task.MipLevel = level;
        task.Region = region;
        task.OnCompleted += completeCallback;
        Device.Tasks.Push(priority, task);
    }

    public unsafe void SetSubResourceData<T>(GpuPriority priority, uint level, T* data, uint startIndex, uint count, uint pitch, uint arrayIndex = 0,
        GpuTask.EventHandler completeCallback = null)
        where T : unmanaged
    {
        TextureSetSubResourceTask<T> task = new();
        task.Initialize(data, (uint)sizeof(T), startIndex, count);
        task.Pitch = pitch;
        task.Resource = this;
        task.ArrayIndex = arrayIndex;
        task.MipLevel = level;
        task.OnCompleted += completeCallback;
        Device.Tasks.Push(priority, task);
    }

    /// <inheritdoc/>
    public void GetData(GpuPriority priority, Action<TextureData> callback)
    {
        TextureGetDataTask task = Device.Tasks.Get<TextureGetDataTask>();
        task.OnGetData = callback;
        Device.Tasks.Push( priority, task);
    }

    public void GetSubResourceData(GpuPriority priority, uint mipLevel, uint arrayIndex, Action<TextureSlice> callback)
    {
        TextureGetSliceTask task = Device.Tasks.Get<TextureGetSliceTask>();
        task.OnGetData = callback;
        task.Resource = this;
        task.MipMapLevel = mipLevel;
        task.ArrayIndex = arrayIndex;
        Device.Tasks.Push(priority, task);
    }

    protected abstract void OnResizeTextureImmediate(GpuCommandList cmd, ref readonly TextureDimensions dimensions, GpuResourceFormat format);

    /// <summary>Gets whether or not the texture is using a supported block-compressed format.</summary>
    public bool IsBlockCompressed { get; protected set; }

    /// <summary>Gets the width of the texture.</summary>
    public uint Width => _dimensions.Width;

    /// <summary>Gets the height of the texture.</summary>
    public uint Height => _dimensions.Height;

    /// <summary>Gets the depth of the texture. For a 3D texture this is the number of slices.</summary>
    public uint Depth => _dimensions.Depth;

    /// <summary>Gets the number of mip map levels in the texture.</summary>
    public uint MipMapCount => _dimensions.MipMapCount;

    /// <summary>Gets the number of array slices in the texture. For a cube-map, this value will a multiple of 6. For example, a cube map with 2 array elements will have 12 array slices.</summary>
    public uint ArraySize => _dimensions.ArraySize;

    /// <summary>
    /// Gets the dimensions of the texture.
    /// </summary>
    public TextureDimensions Dimensions
    {
        get => _dimensions;
        protected set => _dimensions = value;
    }

    /// <inheritdoc/>
    public override ulong SizeInBytes { get; protected set; }

    /// <summary>
    /// Gets the number of samples used when sampling the texture. Anything greater than 1 is considered as multi-sampled. 
    /// </summary>
    public AntiAliasLevel MultiSampleLevel { get; protected set; }

    /// <summary>
    /// Gets whether or not the texture is multisampled. This is true if <see cref="MultiSampleLevel"/> is at least <see cref="AntiAliasLevel.X2"/>.
    /// </summary>
    public bool IsMultisampled => MultiSampleLevel >= AntiAliasLevel.X2;

    /// <inheritdoc/>
    public MSAAQuality SampleQuality { get; protected set; }

    /// <inheritdoc/>
    public override GpuResourceFormat ResourceFormat
    {
        get => _format;
        protected set
        {
            if (_format != value)
            {
                _format = value;
                IsBlockCompressed = BCHelper.GetBlockCompressed(_format);

                if (IsBlockCompressed)
                    SizeInBytes = BCHelper.GetBCSize(_format, Width, Height, MipMapCount) * ArraySize;
                else
                    SizeInBytes = (ResourceFormat.BytesPerPixel() * (Width * Height)) * ArraySize;
            }
        }
    }

    /// <summary>
    /// Gets the ID of the frame that the current <see cref="GpuTexture"/> was resized. 
    /// If the texture was never resized then the frame ID will be the ID of the frame that the texture was created.
    /// </summary>
    public ulong LastFrameResizedID { get; internal set; }
}
