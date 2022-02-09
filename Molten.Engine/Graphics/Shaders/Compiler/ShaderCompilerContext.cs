﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class ShaderCompilerContext
    {
        /// <summary>
        /// HLSL shader objects stored by entry-point name
        /// </summary>
        public Dictionary<string, ShaderCompileResult> Shaders { get; } 

        public ShaderCompileResult Result { get; }

        public IReadOnlyList<ShaderCompilerMessage> Messages { get; }

        public bool HasErrors { get; private set; }

        public ShaderSource Source { get; set; }

        public ShaderCompileFlags Flags { get; }

        List<ShaderCompilerMessage> _messages;
        Dictionary<Type, Dictionary<string, object>> _resources;

        public ShaderCompilerContext()
        {
            _messages = new List<ShaderCompilerMessage>();
            Messages = _messages.AsReadOnly();
            Shaders = new Dictionary<string, ShaderCompileResult>();
            Result = new ShaderCompileResult();
        }

        public void AddResource<T>(string name, T resource) 
            where T : EngineObject
        {
            if (_resources.TryGetValue(typeof(T), out Dictionary<string, object> lookup))
            {
                lookup = new Dictionary<string, object>();
                _resources.Add(typeof(T), lookup);
            }

            lookup.Add(name, resource);
        }

        public T TryGetResource<T>(string name)
            where T : EngineObject
        {
            if (_resources.TryGetValue(typeof(T), out Dictionary<string, object> lookup))
            {
                lookup = new Dictionary<string, object>();
                _resources.Add(typeof(T), lookup);
            }

            if (lookup.TryGetValue(name, out object value))
                return value as T;
            else
                return default(T);
        }

        public void AddMessage(string text, ShaderCompilerMessage.Kind type = ShaderCompilerMessage.Kind.Message)
        {
            _messages.Add(new ShaderCompilerMessage()
            {
                Text = $"[{type}] {text}",
                MessageType = type,
            });

            if (type == ShaderCompilerMessage.Kind.Error)
                HasErrors = true;
        }

        public void AddError(string text)
        {
            AddMessage(text, ShaderCompilerMessage.Kind.Error);
        }

        public void AddDebug(string text)
        {
            AddMessage(text, ShaderCompilerMessage.Kind.Debug);
        }

        public void AddWarning(string text)
        {
            AddMessage(text, ShaderCompilerMessage.Kind.Warning);
        }

        public abstract void ParseFlags(ShaderCompileFlags flags);
    }
}