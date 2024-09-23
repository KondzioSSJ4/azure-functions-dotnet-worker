//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using System.Threading;
//using Microsoft.Azure.Functions.Worker.Sdk.Generators.Extensions;
//using Microsoft.CodeAnalysis;

//namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator.BindingGenerator.Http
//{
//    internal sealed class HttpTriggerParser : IGenerateableBindingGenerator
//    {
//        private readonly IMethodSymbol _methodSymbol;
//        private readonly AttributeData _attribute;
//        private readonly IParameterSymbol _parameter;
//        private readonly List<Diagnostic> _diagnostics = new();

//        public IReadOnlyCollection<Diagnostic> ParsedDiagnostics => _diagnostics;

//        public HttpTriggerParser(
//            IMethodSymbol methodSymbol,
//            AttributeData attribute,
//            IParameterSymbol parameter)
//        {
//            _methodSymbol = methodSymbol;
//            _attribute = attribute;
//            _parameter = parameter;
//        }

//        public IEnumerable<IGenerateableBinding> Generate(CancellationToken cancellationToken)
//        {
//            var methods = GetMethods();
//            var authLevel = GetAuthorizationLevel();
//            var argumentName = _parameter.Name;
//            var route = _attribute.GetArgumentByName("Route");

//            yield return new TriggerBinding()
//            {
//                ArgumentName = argumentName,
//                Methods = methods,
//                AuthorizationLevel = authLevel,
//                Route = route.HasValue && !route.Value.IsNull
//                    ? route.Value.Value.ToString()
//                    : null,
//                IsRetrySupported = _attribute.IsRetrySupported()
//            };
//        }

//        private int? GetAuthorizationLevel()
//        {
//            var level = _attribute.ConstructorArguments
//                .Where(x => x.Kind == TypedConstantKind.Enum)
//                .Select(x => x.Value?.ToString())
//                .FirstOrDefault();

//            return int.TryParse(level, out var levelInt)
//                ? levelInt
//                : null;
//        }

//        private string[]? GetMethods()
//        {
//            var methodArgument = _attribute.ConstructorArguments
//                .FirstOrDefault(x => x.Type is not null
//                    && x.Type.Kind == SymbolKind.ArrayType);

//            if (methodArgument.IsNull)
//            {
//                return null;
//            }

//            var methods = methodArgument.Values.Select(x => x.Value.ToString()).ToArray();
//            return methods.Length == 0
//                ? null
//                : methods;
//        }

//        public sealed class TriggerBinding : IGenerateableBinding
//        {
//            public string ArgumentName { get; internal set; }
//            public string[]? Methods { get; internal set; }
//            public int? AuthorizationLevel { get; internal set; }
//            public string? Route { get; internal set; }

//            public string BindingName => "httpTrigger";

//            public BindingType BindingType => BindingType.Trigger;

//            public bool IsParsable => true;

//            public bool IsRetrySupported { get; internal set; }

//            public ParsedType RawType => throw new NotImplementedException();

//            public string ToGeneratedBinding()
//            {
//                return ToString(); // TODO
//            }

//            public string ToRawBinding()
//            {
//                return JsonSerializer.Serialize(new JsonRawBinding()
//                {
//                    Name = ArgumentName,
//                    AuthLevel = AuthorizationLevel switch
//                    {
//                        null => null,
//                        0 => "Anonymous",
//                        1 => "User",
//                        2 => "Function",
//                        3 => "System",
//                        4 => "Admin",
//                        _ => throw new NotImplementedException($"Unknown level {AuthorizationLevel}")
//                    },
//                    Direction = "In",
//                    Methods = Methods,
//                    Route = Route,
//                    Type = BindingName
//                },
//                new JsonSerializerOptions()
//                {
//                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
//                });
//            }

//            public override string ToString()
//            {
//                return $"Method: {string.Join(";", Methods ?? Array.Empty<string>())}, AuthLevel: {AuthorizationLevel}";
//            }

//            string IGenerateableBinding.ToGeneratedBinding()
//            {
//                throw new NotImplementedException();
//            }

//            string IGenerateableBinding.ToRawBinding()
//            {
//                throw new NotImplementedException();
//            }
//        }

//        private sealed class JsonRawBinding
//        {
//            public string Name { get; set; }
//            public string Type { get; set; }
//            public string Direction { get; set; }

//            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
//            public string? AuthLevel { get; set; }

//            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
//            public IReadOnlyCollection<string>? Methods { get; set; }

//            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
//            public string? Route { get; set; }
//        }
//    }
//}
