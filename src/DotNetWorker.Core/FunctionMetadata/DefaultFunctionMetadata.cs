// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Functions.Worker.Core.FunctionMetadata
{
    public enum FunctionBindingDirection
    {
        In = 1,
        Out,
        InOut
    }

    public interface IGeneratedBinding
    {
        string Name { get; }
        FunctionBindingDirection Direction { get; }
        string? BindingType { get; }
        string? DataType { get; }
        IReadOnlyDictionary<string, string> Properties { get; }
    }

    public sealed class DefaultGeneratedBinding : IGeneratedBinding
    {
        public DefaultGeneratedBinding(
            string name,
            FunctionBindingDirection direction,
            string? bindingType,
            string? dataType,
            IReadOnlyDictionary<string, string> properties)
        {
            Name = name;
            Direction = direction;
            BindingType = bindingType;
            DataType = dataType;
            Properties = properties;
        }

        public string Name { get; }

        public FunctionBindingDirection Direction { get; }

        public string? BindingType { get; }

        public string? DataType { get; }

        public IReadOnlyDictionary<string, string> Properties { get; }
    }

    public interface IGeneratedFunctionMetadata : IFunctionMetadata
    {
        IReadOnlyCollection<IGeneratedBinding> GeneratedBindings { get; }
    }

    public sealed class SourceGeneratedFunctionMetadata : IGeneratedFunctionMetadata
    {
        public SourceGeneratedFunctionMetadata(
            string? functionId,
            bool isProxy,
            string? language,
            bool managedDependencyEnabled,
            string? name,
            string? entryPoint,
            string? scriptFile,
            IRetryOptions? retry,
            IList<string>? rawBindings,
            IReadOnlyCollection<IGeneratedBinding> generatedBindings)
        {
            FunctionId = functionId;
            IsProxy = isProxy;
            Language = language;
            ManagedDependencyEnabled = managedDependencyEnabled;
            Name = name;
            EntryPoint = entryPoint;
            RawBindings = rawBindings;
            ScriptFile = scriptFile;
            Retry = retry;
            GeneratedBindings = generatedBindings;
        }

        public string? FunctionId { get; }
        public bool IsProxy { get; }
        public string? Language { get; }
        public bool ManagedDependencyEnabled { get; }
        public string? Name { get; }
        public string? EntryPoint { get; }
        public IList<string>? RawBindings { get; }
        public string? ScriptFile { get; }
        public IRetryOptions? Retry { get; }

        public IReadOnlyCollection<IGeneratedBinding> GeneratedBindings { get; }
    }

    /// <summary>
    /// Local representation of FunctionMetadata
    /// </summary>
    public class DefaultFunctionMetadata : IFunctionMetadata
    {
        private string? _functionId;
        private string? _name;
        private string? _entryPoint;
        private string? _scriptFile;

        /// <inheritdoc/>
        public string? FunctionId
        {
            get
            {
                _functionId ??= HashFunctionId(this);
                return _functionId;
            }
        }

        /// <inheritdoc/>
        public bool IsProxy { get; set; }

        /// <inheritdoc/>
        public string? Language { get; set; }

        /// <inheritdoc/>
        public bool ManagedDependencyEnabled { get; set; }

        /// <inheritdoc/>
        public string? Name { get => _name; set => ClearIdAndSet(value, ref _name); }

        /// <inheritdoc/>
        public string? EntryPoint { get => _entryPoint; set => ClearIdAndSet(value, ref _entryPoint); }

        /// <inheritdoc/>
        public IList<string>? RawBindings { get; set; }

        /// <inheritdoc/>
        public string? ScriptFile { get => _scriptFile; set => ClearIdAndSet(value, ref _scriptFile); }

        /// <inheritdoc/>
        public IRetryOptions? Retry { get; set; }

        private static string? HashFunctionId(DefaultFunctionMetadata function)
        {
            // We use uint to avoid the '-' sign when we .ToString() the result.
            // This function is adapted from https://github.com/Azure/azure-functions-host/blob/71ecbb2c303214f96d7e17310681fd717180bdbb/src/WebJobs.Script/Utility.cs#L847-L863
            static uint GetStableHash(string value)
            {
                unchecked
                {
                    uint hash = 23;
                    foreach (char c in value)
                    {
                        hash = (hash * 31) + c;
                    }

                    return hash;
                }
            }

            unchecked
            {
                bool atLeastOnePresent = false;
                uint hash = 17;

                if (function.Name is not null)
                {
                    atLeastOnePresent = true;
                    hash = hash * 31 + GetStableHash(function.Name);
                }

                if (function.ScriptFile is not null)
                {
                    atLeastOnePresent = true;
                    hash = hash * 31 + GetStableHash(function.ScriptFile);
                }

                if (function.EntryPoint is not null)
                {
                    atLeastOnePresent = true;
                    hash = hash * 31 + GetStableHash(function.EntryPoint);
                }

                return atLeastOnePresent ? hash.ToString() : null;
            }
        }

        private void ClearIdAndSet(string? value, ref string? field)
        {
            if (!StringComparer.Ordinal.Equals(value, field))
            {
                _functionId = null;
            }

            field = value;
        }
    }
}
