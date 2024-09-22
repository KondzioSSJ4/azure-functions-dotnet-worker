// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Text.Json;
using Google.Protobuf.Collections;
using Microsoft.Azure.Functions.Worker.Core.FunctionMetadata;
using Microsoft.Azure.Functions.Worker.Grpc.Messages;

namespace Microsoft.Azure.Functions.Worker.Grpc.FunctionMetadata
{
    internal static class FunctionMetadataRpcExtensions
    {
        internal static MapField<string, BindingInfo> GetBindingInfoList(this IFunctionMetadata funcMetadata)
        {
            if (funcMetadata is RpcFunctionMetadata rpcFuncMetadata)
            {
                return rpcFuncMetadata.Bindings;
            }

            if (funcMetadata is IGeneratedFunctionMetadata generatedMetadata)
            {
                return GetFields(generatedMetadata);
            }

            return ParseFields(funcMetadata);
        }

        private static MapField<string, BindingInfo> ParseFields(IFunctionMetadata funcMetadata)
        {
            var bindings = new MapField<string, BindingInfo>();
            var rawBindings = funcMetadata.RawBindings;

            if (rawBindings is null || rawBindings.Count == 0)
            {
                throw new FormatException("At least one binding must be declared in a Function.");
            }

            foreach (var bindingJson in rawBindings)
            {
                var binding = JsonSerializer.Deserialize<JsonElement>(bindingJson);
                BindingInfo bindingInfo = CreateBindingInfo(binding);
                binding.TryGetProperty("name", out JsonElement jsonName);
                bindings.Add(jsonName.ToString()!, bindingInfo);
            }

            return bindings;
        }

        internal static BindingInfo CreateBindingInfo(JsonElement binding)
        {
            var hasDirection = binding.TryGetProperty("direction", out JsonElement jsonDirection);
            var hasType = binding.TryGetProperty("type", out JsonElement jsonType);

            if (!hasDirection
                || !hasType
                || !Enum.TryParse(jsonDirection.ToString()!, out BindingInfo.Types.Direction direction))
            {
                throw new FormatException("Bindings must declare a direction and type.");
            }

            BindingInfo bindingInfo = new BindingInfo
            {
                Direction = direction,
                Type = jsonType.ToString()
            };

            var hasDataType = binding.TryGetProperty("dataType", out JsonElement jsonDataType);

            if (hasDataType)
            {
                if (!Enum.TryParse(jsonDataType.ToString()!, out BindingInfo.Types.DataType dataType))
                {
                    throw new FormatException("Invalid DataType for a binding.");
                }

                bindingInfo.DataType = dataType;
            }

            return bindingInfo;
        }

        private static MapField<string, BindingInfo> GetFields(
            IGeneratedFunctionMetadata generatedMetadata)
        {
            var bindings = new MapField<string, BindingInfo>();
            var rawBindings = generatedMetadata.GeneratedBindings;

            if (rawBindings is null || rawBindings.Count == 0)
            {
                throw new FormatException("At least one binding must be declared in a Function.");
            }

            foreach (var item in rawBindings)
            {
                bindings.Add(item.Name, CreateBindingInfo(item));
            }

            return bindings;
        }

        private static BindingInfo CreateBindingInfo(IGeneratedBinding item)
        {
            if (string.IsNullOrWhiteSpace(item.DataType)
                || !Enum.TryParse(item.DataType, out BindingInfo.Types.DataType dataType))
            {
                throw new FormatException($"Invalid DataType for a binding: {item.DataType}");
            }

            var info = new BindingInfo()
            {
                Direction = item.Direction switch
                {
                    FunctionBindingDirection.In => BindingInfo.Types.Direction.In,
                    FunctionBindingDirection.Out => BindingInfo.Types.Direction.Out,
                    FunctionBindingDirection.InOut => BindingInfo.Types.Direction.Inout,

                    _ => throw new FormatException($"Unknown binding {item.Direction} ({(int)item.Direction})")
                },
                Type = item.BindingType,
                DataType = dataType
            };

            if (item.Properties?.Count > 0)
            {
                foreach (var pair in item.Properties)
                {
                    info.Properties[pair.Key] = pair.Value;
                }
            }

            return info;
        }
    }
}
