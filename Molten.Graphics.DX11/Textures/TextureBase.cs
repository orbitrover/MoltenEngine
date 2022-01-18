﻿using System;
using System.Collections.Generic;
using System.Linq;
using Molten.Collections;
using System.Runtime.InteropServices;
using Molten.Graphics.Textures;
using Silk.NET.DXGI;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public delegate void TextureEvent(TextureBase texture);

    public unsafe abstract partial class TextureBase : PipeBindableResource, ITexture
    {
        ThreadedQueue<ITextureChange> _pendingChanges;

        /// <summary>Triggered right before the internal texture resource is created.</summary>
        public event TextureEvent OnPreCreate;

        /// <summary>Triggered after the internal texture resource has been created.</summary>
        public event TextureEvent OnCreate;

        /// <summary>Triggered if the creation of the internal texture resource has failed (resulted in a null resource).</summary>
        public event TextureEvent OnCreateFailed;

        /// <summary>
        /// Invokved right before resizing of the texture begins.
        /// </summary>
        public event TextureHandler OnPreResize;

        /// <summary>
        /// Invoked after resizing of the texture has completed.
        /// </summary>
        public event TextureHandler OnPostResize;

        ID3D11Resource* _native;
        RendererDX11 _renderer;

        ShaderResourceViewDesc _srvDescription;
        UnorderedAccessViewDesc _uavDescription;

        internal TextureBase(RendererDX11 renderer, int width, int height, int depth, int mipCount, 
            int arraySize, int sampleCount, Format format, TextureFlags flags) : base(renderer.Device)
        {
            _renderer = renderer;
            Flags = flags;
            ValidateFlagCombination();

            _pendingChanges = new ThreadedQueue<ITextureChange>();

            Width = width;
            Height = height;
            Depth = depth;
            MipMapCount = mipCount;
            ArraySize = arraySize;
            SampleCount = sampleCount;
            DxgiFormat = format;
            IsValid = false;

            _srvDescription = new ShaderResourceViewDesc();
            IsBlockCompressed = BCHelper.GetBlockCompressed(DxgiFormat.FromApi());
        }

        public Texture1DProperties Get1DProperties()
        {
            return new Texture1DProperties()
            {
                Width = Width,
                ArraySize = ArraySize,
                Flags = Flags,
                Format = DataFormat,
                MipMapLevels = MipMapCount,
            };
        }

        protected void RaisePostResizeEvent()
        {
            OnPostResize?.Invoke(this);
        }

        private void ValidateFlagCombination()
        {
            // Validate RT mip-maps
            if (HasFlags(TextureFlags.AllowMipMapGeneration))
            {
                if(HasFlags(TextureFlags.NoShaderResource) || !(this is RenderSurface))
                    throw new TextureFlagException(Flags, "Mip-map generation is only available on render-surface shader resources.");
            }

            if (HasFlags(TextureFlags.Staging))
            {
                if (Flags != (TextureFlags.Staging) && Flags != (TextureFlags.Staging | TextureFlags.NoShaderResource))
                    throw new TextureFlagException(Flags, "Staging textures cannot have other flags set except NoShaderResource.");

                Flags |= TextureFlags.NoShaderResource;
            }
        }

        protected BindFlag GetBindFlags()
        {
            BindFlag result = 0;

            if (HasFlags(TextureFlags.AllowUAV))
                result |= BindFlag.BindUnorderedAccess;

            if (!HasFlags(TextureFlags.NoShaderResource))
                result |= BindFlag.BindShaderResource;

            if (this is RenderSurface)
                result |= BindFlag.BindRenderTarget;

            if (this is DepthStencilSurface)
                result |= BindFlag.BindDepthStencil;

            return result;
        }

        protected ResourceMiscFlag GetResourceFlags()
        {
            ResourceMiscFlag result = 0;

            if (HasFlags(TextureFlags.SharedResource))
                result |= ResourceMiscFlag.ResourceMiscShared;

            if (HasFlags(TextureFlags.AllowMipMapGeneration))
                result |= ResourceMiscFlag.ResourceMiscGenerateMips;

            return result;
        }

        protected Usage GetUsageFlags()
        {
            if (HasFlags(TextureFlags.Staging))
                return Usage.UsageStaging;
            else if (HasFlags(TextureFlags.Dynamic))
                return Usage.UsageDynamic;
            else
                return Usage.UsageDefault;
        }

        protected CpuAccessFlag GetAccessFlags()
        {
            if (HasFlags(TextureFlags.Staging))
                return CpuAccessFlag.CpuAccessRead;
            else if (HasFlags(TextureFlags.Dynamic))
                return CpuAccessFlag.CpuAccessWrite;
            else
                return 0;
        }

        protected void CreateTexture(bool resize)
        {
            OnPreCreate?.Invoke(this);

            // Dispose of old resources
            OnDisposeForRecreation();
            _native = CreateResource(resize);

            if (_native != null)
            {
                SilkUtil.ReleasePtr(ref UAV);
                SilkUtil.ReleasePtr(ref SRV);

                //TrackAllocation();

                if (!HasFlags(TextureFlags.NoShaderResource))
                {
                    SetSRVDescription(ref _srvDescription);
                    Device.Native->CreateShaderResourceView(_native, ref _srvDescription, ref SRV);
                }

                if (HasFlags(TextureFlags.AllowUAV))
                {
                    SetUAVDescription(ref _srvDescription, ref _uavDescription);
                    Device.Native->CreateUnorderedAccessView(_native, ref _uavDescription, ref UAV);
                }

                OnCreate?.Invoke(this);
            }
            else
            {
                OnCreateFailed?.Invoke(this);
            }

            IsValid = _native != null;
        }

        protected abstract void SetUAVDescription(ref ShaderResourceViewDesc srvDesc, ref UnorderedAccessViewDesc desc);

        protected abstract void SetSRVDescription(ref ShaderResourceViewDesc desc);

        protected virtual void OnDisposeForRecreation()
        {
            PipelineDispose();
        }


        internal override void PipelineDispose()
        {
            base.PipelineDispose();

            //TrackDeallocation();
            SilkUtil.ReleasePtr(ref _native);
        }

        public bool HasFlags(TextureFlags flags)
        {
            return (Flags & flags) == flags;
        }

        /// <summary>Generates mip maps for the texture via the provided <see cref="PipeDX11"/>.</summary>
        public void GenerateMipMaps()
        {
            if (!((Flags & TextureFlags.AllowMipMapGeneration) == TextureFlags.AllowMipMapGeneration))
                throw new Exception("Cannot generate mip-maps for texture. Must have flag: TextureFlags.AllowMipMapGeneration.");

            TexturegenMipMaps change = new TexturegenMipMaps();
            _pendingChanges.Enqueue(change);
        }

        internal void GenerateMipMaps(PipeDX11 pipe)
        {
            if (SRV != null)
                pipe.Context->GenerateMips(SRV);
        }

        public void SetData<T>(Rectangle area, T[] data, int bytesPerPixel, int level, int arrayIndex = 0) where T : struct
        {
            int count = data.Length;
            int texturePitch = area.Width * bytesPerPixel;
            int pixels = area.Width * area.Height;

            int expectedBytes = pixels * bytesPerPixel;
            int dataBytes = data.Length * Marshal.SizeOf(typeof(T));

            if (pixels != data.Length)
                throw new Exception($"The provided data does not match the provided area of {area.Width}x{area.Height}. Expected {expectedBytes} bytes. {dataBytes} bytes were provided.");

            // Do a bounds check
            Rectangle texBounds = new Rectangle(0, 0, Width, Height);
            if (!texBounds.Contains(area))
                throw new Exception("The provided area would go outside of the current texture's bounds.");

            TextureSet<T> change = new TextureSet<T>()
            {
                Stride = Marshal.SizeOf(typeof(T)),
                Count = count,
                Data = new T[count],
                Pitch = texturePitch,
                StartIndex = 0,
                ArrayIndex = arrayIndex,
                MipLevel = level,
                Area = area,
            };

            //copy the data so that it is not affected by other threads
            Array.Copy(data, 0, change.Data, 0, count);

            _pendingChanges.Enqueue(change);
        }

        /// <summary>Copies data fom the provided <see cref="TextureData"/> instance into the current texture.</summary>
        /// <param name="data"></param>
        /// <param name="srcMipIndex">The starting mip-map index within the provided <see cref="TextureData"/>.</param>
        /// <param name="srcArraySlice">The starting array slice index within the provided <see cref="TextureData"/>.</param>
        /// <param name="mipCount">The number of mip-map levels to copy per array slice, from the provided <see cref="TextureData"/>.</param>
        /// <param name="arrayCount">The number of array slices to copy from the provided <see cref="TextureData"/>.</param>
        /// <param name="destMipIndex">The mip-map index within the current texture to start copying to.</param>
        /// <param name="destArraySlice">The array slice index within the current texture to start copying to.<</param>
        public void SetData(TextureData data, int srcMipIndex, int srcArraySlice, int mipCount,
            int arrayCount, int destMipIndex = 0, int destArraySlice = 0)
        {
            TextureData.Slice level = null;

            for(int a = 0; a < arrayCount; a++)
            {
                for(int m = 0; m < mipCount; m++)
                {
                    int slice = srcArraySlice + a;
                    int mip = srcMipIndex + m;
                    int dataID = TextureData.GetLevelID(data.MipMapLevels, mip, slice);
                    level = data.Levels[dataID];

                    if (level.TotalBytes == 0)
                        continue;

                    int destSlice = destArraySlice + a;
                    int destMip = destMipIndex + m;
                    SetData(destMip, level.Data, 0, level.TotalBytes, level.Pitch, destSlice);
                }
            }
        }

        public void SetData(TextureData.Slice data, int mipIndex, int arraySlice)
        {
            TextureSet<byte> change = new TextureSet<byte>()
            {
                Stride = 1,
                Count = data.TotalBytes,
                Data = data.Data.Clone() as byte[],
                Pitch = data.Pitch,
                StartIndex = 0,
                ArrayIndex = arraySlice,
                MipLevel = mipIndex,
            };

            // Store pending change.
            _pendingChanges.Enqueue(change);
        }

        public void SetData<T>(int level, T[] data, int startIndex, int count, int pitch, int arrayIndex) where T : struct
        {
            TextureSet<T> change = new TextureSet<T>()
            {
                Stride = (int)Marshal.SizeOf(typeof(T)),
                Count = count,
                Data = new T[count],
                Pitch = pitch,
                StartIndex = startIndex,
                ArrayIndex = arrayIndex,
                MipLevel = level,
            };

            //copy the data so that it is not affected by other threads
            Array.Copy(data, startIndex, change.Data, 0, count);

            // Store pending change.
            _pendingChanges.Enqueue(change);
        }

        public void GetData(ITexture stagingTexture, Action<TextureData> callback)
        {
            _pendingChanges.Enqueue(new TextureGet()
            {
                StagingTexture = stagingTexture as TextureBase,
                Callback = callback,
            });
        }

        public void GetData(ITexture stagingTexture, int mipLevel, int arrayIndex, Action<TextureData.Slice> callback)
        {
            _pendingChanges.Enqueue(new TextureGetSlice()
            {
                StagingTexture = stagingTexture as TextureBase,
                Callback = callback,
                ArrayIndex = arrayIndex,
                MipMapLevel = mipLevel,
            });
        }

        internal TextureData GetAllData(PipeDX11 pipe, TextureBase staging)
        {
            if (staging == null && !HasFlags(TextureFlags.Staging))
                throw new TextureCopyException(this, null, "A null staging texture was provided, but this is only valid if the current texture is a staging texture. A staging texture is required to retrieve data from non-staged textures.");

            if (!staging.HasFlags(TextureFlags.Staging))
                throw new TextureFlagException(staging.Flags, "Provided staging texture does not have the staging flag set.");

            // Validate dimensions.
            if (staging.Width != Width ||
                staging.Height != Height ||
                staging.Depth != Depth)
                throw new TextureCopyException(this, staging, "Staging texture dimensions do not match current texture.");

            staging.Apply(pipe);

            ID3D11Resource* resToMap = _native;

            if (staging != null)
            {
                pipe.Context->CopyResource(staging.NativePtr, _native);
                pipe.Profiler.Current.CopyResourceCount++;
                resToMap = staging._native;
            }

            TextureData data = new TextureData()
            {
                ArraySize = ArraySize,
                Flags = Flags,
                Format = DataFormat,
                Height = Height,
                HighestMipMap = 0,
                IsCompressed = IsBlockCompressed,
                Levels = new TextureData.Slice[ArraySize * MipMapCount],
                MipMapLevels = MipMapCount,
                Width = Width,
            };

            int blockSize = BCHelper.GetBlockSize(DataFormat);
            int expectedRowPitch = 4 * Width; // 4-bytes per pixel * Width.
            int expectedSlicePitch = expectedRowPitch * Height;

            // Iterate over each array slice.
            for (int a = 0; a < ArraySize; a++)
            {
                // Iterate over all mip-map levels of the array slice.
                for (int i = 0; i < MipMapCount; i++)
                {
                    int subID = (a * MipMapCount) + i;
                    data.Levels[subID] = GetSliceData(pipe, staging, i, a);
                }
            }

            return data;
        }

        /// <summary>A private helper method for retrieving the data of a subresource.</summary>
        /// <param name="pipe">The pipe to perform the retrieval.</param>
        /// <param name="staging">The staging texture to copy the data to.</param>
        /// <param name="level">The mip-map level.</param>
        /// <param name="arraySlice">The array slice.</param>
        /// <returns></returns>
        internal unsafe TextureData.Slice GetSliceData(PipeDX11 pipe, TextureBase staging, int level, int arraySlice)
        {
            int subID = (arraySlice * MipMapCount) + level;
            int subWidth = Width >> (int)level;
            int subHeight = Height >> (int)level;

            ID3D11Resource* resToMap = _native;

            if (staging != null)
            {
                pipe.CopyResourceRegion(_native, (uint)subID, null, staging._native, (uint)subID, Vector3UI.Zero);
                pipe.Profiler.Current.CopySubresourceCount++;
                resToMap = staging._native;
            }

            // Now pull data from it
            DataBox databox = pipe.Context.MapSubresource(resToMap, subID, Map.MapRead, 0);
            // NOTE: Databox: "The row pitch in the mapping indicate the offsets you need to use to jump between rows."
            // https://gamedev.stackexchange.com/questions/106308/problem-with-id3d11devicecontextcopyresource-method-how-to-properly-read-a-t/106347#106347


            int blockSize = BCHelper.GetBlockSize(DataFormat);
            int expectedRowPitch = 4 * Width; // 4-bytes per pixel * Width.
            int expectedSlicePitch = expectedRowPitch * Height;

            if (blockSize > 0)
                BCHelper.GetBCLevelSizeAndPitch(subWidth, subHeight, blockSize, out expectedSlicePitch, out expectedRowPitch);

            byte[] sliceData = new byte[expectedSlicePitch];
            fixed (byte* ptrFixedSlice = sliceData)
            {
                byte* ptrSlice = ptrFixedSlice;
                byte* ptrDatabox = (byte*)databox.DataPointer.ToPointer();

                int p = 0;
                while (p < databox.SlicePitch)
                {
                    Buffer.MemoryCopy(ptrDatabox, ptrSlice, expectedSlicePitch, expectedRowPitch);
                    ptrDatabox += databox.RowPitch;
                    ptrSlice += expectedRowPitch;
                    p += databox.RowPitch;
                }
            }
            pipe.UnmapResource(_native, (uint)subID);

            TextureData.Slice slice = new TextureData.Slice()
            {
                Width = subWidth,
                Height = subHeight,
                Data = sliceData,
                Pitch = expectedRowPitch,
                TotalBytes = expectedSlicePitch,
            };

            return slice;
        }

        internal void SetSizeInternal(int newWidth, int newHeight, int newDepth, int newMipMapCount, int newArraySize, Format newFormat)
        {
            // Avoid resizing/recreation if nothing has actually changed.
            if (Width == newWidth && 
                Height == newHeight && 
                Depth == newDepth && 
                MipMapCount == newMipMapCount && 
                ArraySize == newArraySize && 
                DxgiFormat == newFormat)
                return;

            OnPreResize?.Invoke(this);
            Width = Math.Max(1, newWidth);
            Height = Math.Max(1, newHeight);
            Depth = Math.Max(1, newDepth);
            MipMapCount = Math.Max(1, newMipMapCount);
            DxgiFormat = newFormat;

            UpdateDescription(Width, Height, Depth, Math.Max(1, newMipMapCount), Math.Max(1, newArraySize), newFormat);
            CreateTexture(true);
            OnPostResize?.Invoke(this);
        }


        protected virtual void UpdateDescription(int newWidth, int newHeight, 
            int newDepth, int newMipMapCount, int newArraySize, Format newFormat) { }

        protected abstract ID3D11Resource* CreateResource(bool resize);

        private protected void QueueChange(ITextureChange change)
        {
            _pendingChanges.Enqueue(change);
        }

        public void Resize(int newWidth)
        {
            Resize(newWidth, MipMapCount, DxgiFormat.FromApi());
        }

        public void Resize(int newWidth, int newMipMapCount, GraphicsFormat newFormat)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = Height,
                NewMipMapCount = newMipMapCount,
                NewArraySize = ArraySize,
                NewFormat = DxgiFormat,
            });
        }

        public void CopyTo(ITexture destination)
        {
            TextureBase destTexture = destination as TextureBase;

            if (this.DataFormat != destination.DataFormat)
                throw new TextureCopyException(this, destTexture, "The source and destination texture formats do not match.");

            // Validate dimensions.
            if (destTexture.Width != this.Width ||
                destTexture.Height != this.Height ||
                destTexture.Depth != this.Depth)
                throw new TextureCopyException(this, destTexture, "The source and destination textures must have the same dimensions.");

            QueueChange(new TextureCopyTo()
            {
                Destination = destination as TextureBase,
            });

            TextureApply applyTask = TextureApply.Get();
            applyTask.Texture = this;
            _renderer.PushTask(applyTask);
        }

        public void CopyTo(int sourceLevel, int sourceSlice, ITexture destination, int destLevel, int destSlice)
        {
            TextureBase destTexture = destination as TextureBase;

            if (destination.HasFlags(TextureFlags.Dynamic))
                throw new TextureCopyException(this, destTexture, "Cannot copy to a dynamic texture via GPU. GPU cannot write to dynamic textures.");

            if (this.DataFormat != destination.DataFormat)
                throw new TextureCopyException(this, destTexture, "The source and destination texture formats do not match.");

            // Validate dimensions.
            // TODO this should only test the source and destination level dimensions, not the textures themselves.
            if (destTexture.Width != this.Width ||
                destTexture.Height != this.Height ||
                destTexture.Depth != this.Depth)
                throw new TextureCopyException(this, destTexture, "The source and destination textures must have the same dimensions.");

            if (sourceLevel >= this.MipMapCount)
                throw new TextureCopyException(this, destTexture, "The source mip-map level exceeds the total number of levels in the source texture.");

            if (sourceSlice >= this.ArraySize)
                throw new TextureCopyException(this, destTexture, "The source array slice exceeds the total number of slices in the source texture.");

            if (destLevel >= destTexture.MipMapCount)
                throw new TextureCopyException(this, destTexture, "The destination mip-map level exceeds the total number of levels in the destination texture.");

            if (destSlice >= destTexture.ArraySize)
                throw new TextureCopyException(this, destTexture, "The destination array slice exceeds the total number of slices in the destination texture.");

            QueueChange(new TextureCopyLevel()
            {
                Destination = destination as TextureBase,
                SourceLevel = sourceLevel,
                SourceSlice = sourceSlice,
                DestinationLevel = destLevel,
                DestinationSlice = destSlice,
            });

            TextureApply applyTask = TextureApply.Get();
            applyTask.Texture = this;
            _renderer.PushTask(applyTask);
        }

        /// <summary>Applies all pending changes to the texture. Take care when calling this method in multi-threaded code. Calling while the
        /// GPU may be using the texture will cause unexpected behaviour.</summary>
        /// <param name="pipe"></param>
        internal void Apply(PipeDX11 pipe)
        {
            if (IsDisposed)
                return;

            if(_native == null)
                CreateTexture(false);

            // process all changes for the current pipe.
            while (_pendingChanges.Count > 0)
            {
                if (_pendingChanges.TryDequeue(out ITextureChange change))
                    change.Process(pipe, this);
            }
        }

        protected internal override void Refresh(PipeSlot slot, PipeDX11 pipe)
        {
            Apply(pipe);
        }

        /// <summary>Gets the flags that were passed in when the texture was created.</summary>
        public TextureFlags Flags { get; protected set; }

        /// <summary>Gets the format of the texture.</summary>
        public Format DxgiFormat { get; protected set; }

        public GraphicsFormat DataFormat => (GraphicsFormat)DxgiFormat;

        /// <summary>Gets whether or not the texture is using a supported block-compressed format.</summary>
        public bool IsBlockCompressed { get; protected set; }

        /// <summary>Gets the width of the texture.</summary>
        public int Width { get; protected set; }

        /// <summary>Gets the height of the texture.</summary>
        public int Height { get; protected set; }

        /// <summary>Gets the depth of the texture. For a 3D texture this is the number of slices.</summary>
        public int Depth { get; protected set; }

        /// <summary>Gets the number of mip map levels in the texture.</summary>
        public int MipMapCount { get; protected set; }

        /// <summary>Gets the number of array slices in the texture. For a cube-map, this value will a multiple of 6. For example, a cube map with 2 array elements will have 12 array slices.</summary>
        public int ArraySize { get; protected set; }

        /// <summary>
        /// Gets the number of samples used when sampling the texture. Anything greater than 1 is considered as multi-sampled. 
        /// </summary>
        public int SampleCount { get; protected set; }

        /// <summary>
        /// Gets whether or not the texture is multisampled. This is true if <see cref="SampleCount"/> is greater than 1.
        /// </summary>
        public bool IsMultisampled => SampleCount > 1;

        public bool IsValid { get; protected set; }

        internal override unsafe ID3D11Resource* NativePtr => _native;

        /// <summary>
        /// Gets the renderer that the texture is bound to.
        /// </summary>
        public RenderService Renderer => _renderer;
    }
}