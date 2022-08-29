﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Native;
using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Extensions;
using Silk.NET.OpenAL.Extensions.Enumeration;

namespace Molten.Audio.OpenAL
{
    public unsafe abstract class AudioDevice : IAudioDevice, IDisposable
    {
        AudioServiceAL _service; 
        Device* _device;
        Dictionary<Type, ContextExtensionBase> _extensions;

        internal AudioDevice(AudioServiceAL service, string specifier, bool isDefault, AudioDeviceType deviceType)
        {
            Name = specifier;
            DeviceType = deviceType;
            IsDefault = isDefault;
            _service = service;
            _extensions = new Dictionary<Type, ContextExtensionBase>();
        }

        protected bool TryGetExtension<T>(out T extension) where T : ContextExtensionBase
        {
            if (!_extensions.TryGetValue(typeof(T), out ContextExtensionBase ext))
            {
                string extName = ExtensionAttribute.GetExtensionAttribute(typeof(T)).Name;
                if (_service.Alc.TryGetExtension<T>(_device, out extension))
                {
                    _extensions.Add(typeof(T), extension);
                    _service.Log.WriteLine($"Loaded device extension '{extName}' for '{Name}'");
                    return true;
                }
                else
                {
                    // See if the OpenAL service has a global version that we can use.
                    if (_service.TryGetExtension(out extension))
                        return true;
                    else
                        Service.Log.Error($"Unable to load device extension {typeof(T).Name} ({extName}) for device '{Name}'");
                }
            }

            extension = null;
            return false;
        }

        internal void TransferTo(AudioDevice other)
        {
            if (other.Service != Service)
                throw new Exception("Devices must be from the same audio service");

            if (other.DeviceType != DeviceType)
                throw new Exception("Device types must be identical");

            if (other == this)
                throw new InvalidOperationException("An audio device cannot be transferred to itself");

            OnTransferTo(other);
        }

        internal void Open()
        {
            if (_device != null)
                throw new AudioDeviceException(this, $"[{DeviceType}] device is already open");

            ContextError result = OnOpen();

            if (result != ContextError.NoError)
            {
                Service.Log.Error($"An error occurred while opening [{DeviceType}] device'{Name}': {result}");
            }
            else
            {
                if (_device == null)
                    throw new AudioDeviceException(this, $"An error occurred while opening [{DeviceType}] device: Ptr was not set");
                else
                    Service.Log.WriteLine($"Opened [{DeviceType}] device '{Name}'");
            }
        }

        internal void Close()
        {
            if (_device == null)
                throw new AudioDeviceException(this, $"[{DeviceType}] device is not open");

            ContextError result = OnClose();

            if (result != ContextError.NoError)
            {
                Service.Log.Error($"An error occurred while closing [{DeviceType}] device '{Name}': {Service.GetErrorMessage(result)}");
            }
            else
            {
                _device = null;
                Service.Log.WriteLine($"Closed [{DeviceType}] device '{Name}'");
            }
        }

        internal void Update(Timing time)
        {
            OnUpdate(time);
        }

        protected abstract ContextError OnOpen();

        protected abstract ContextError OnClose();

        protected abstract void OnUpdate(Timing time);

        /// <summary>
        /// Invoked when the current <see cref="AudioDevice"/> state needs to be transferred to another <see cref="AudioDevice"/> of the same <see cref="DeviceType"/>.
        /// </summary>
        /// <param name="other"></param>
        protected abstract void OnTransferTo(AudioDevice other);

        public void Dispose()
        {
            foreach (ContextExtensionBase ext in _extensions.Values)
                ext.Dispose();

            _extensions.Clear();

            Close();
            GC.SuppressFinalize(this);
        }

        public string Name { get; }

        public bool IsDefault { get; }

        public bool IsCurrent { get; }

        public AudioDeviceType DeviceType { get; }

        public AudioServiceAL Service => _service;

        AudioService IAudioDevice.Service => _service;

        internal ref Device* Ptr => ref _device;
    }
}
