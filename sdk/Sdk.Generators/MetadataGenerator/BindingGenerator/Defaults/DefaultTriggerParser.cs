using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.MetadataGenerator.BindingGenerator.Defaults
{
    internal sealed class DefaultTriggerParser : IGenerateableBindingGenerator
    {
        private const string NameAttribute = "name";
        private const string TypeAttribute = "type";
        private const string DirectionAttribute = "direction";
        private const string IsBatchedAttribute = "isBatched";
        private const string CardinalityAttribute = "cardinality";

        private static readonly IReadOnlyCollection<string> _wellKnowAttributes = new[]
        {
            NameAttribute,
            TypeAttribute,
            DirectionAttribute
        };

        private readonly List<Diagnostic> _diagnostics = new();

        private readonly IMethodSymbol _functionMethodSymbol;
        private readonly SemanticModel _semanticModel;
        private readonly AttributeData _attribute;
        private readonly IParameterSymbol _parameter;

        public DefaultTriggerParser(
            IMethodSymbol functionMethodSymbol,
            SemanticModel semanticModel,
            AttributeData attribute,
            IParameterSymbol parameter)
        {
            _functionMethodSymbol = functionMethodSymbol;
            _semanticModel = semanticModel;
            _attribute = attribute;
            _parameter = parameter;
        }

        public IReadOnlyCollection<Diagnostic> ParsedDiagnostics => _diagnostics;

        public IEnumerable<IGenerateableBinding> Generate(CancellationToken cancellationToken)
        {
            var entries = new Dictionary<string, string>()
            {
                [NameAttribute] = _parameter.Name,
                [TypeAttribute] = GetShortType(),
                [DirectionAttribute] = "In"
            };

            for (var i = 0; i < _attribute.ConstructorArguments.Length; i++)
            {
                var argument = _attribute.ConstructorArguments[i];
                var name = _attribute.AttributeConstructor!.Parameters[i].Name;
                var json = TryGetJsonValue(argument);
                if (json is not null)
                {
                    entries[name.ToCammelCase()] = json;
                }
            }

            foreach (var item in _attribute.NamedArguments)
            {
                var json = TryGetJsonValue(item.Value);
                if (json is not null)
                {
                    entries[item.Key.ToCammelCase()] = json;
                }
            }

            if (!ParsedType.TryParse(
                _parameter.Type,
                out var declaredType,
                out var rawOutputTypeSymbol,
                out var diagnostic)
                || declaredType is null)
            {
                _diagnostics.Add(diagnostic ?? CreateMissingSymbol(TypeAttribute));
                yield break;
            }

            if (declaredType.IsAsyncOperation)
            {
                _diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidBindingType,
                    _parameter.Locations.FirstOrDefault() ?? _functionMethodSymbol.Locations.FirstOrDefault(),
                    new[] { "Task, ValueTask, Task<T>, ValueTask<T>", "input trigger" }));
                yield break;
            }

            var isRetrySupported = _attribute.IsRetrySupported();
            var metadataType = declaredType.GetGeneratorDataType();

            var isBatched = ExtractIsBatchedTrigger(entries);
            if (isBatched)
            {
                var returnType = rawOutputTypeSymbol ?? _parameter.Type;
                if (declaredType.IsAsyncEnumerable)
                {
                    if (!UnwrapAsyncEnumerable(returnType, out metadataType, out diagnostic))
                    {
                        _diagnostics.Add(diagnostic ?? InvalidCardinalityDiagnostic(returnType));
                        yield break;
                    }
                }
                else if (declaredType.IsEnumerable)
                {
                    if (!UnwrapEnumerable(returnType, out metadataType, out diagnostic))
                    {
                        _diagnostics.Add(diagnostic ?? InvalidCardinalityDiagnostic(returnType));
                        yield break;
                    }
                }
                else
                {
                    _diagnostics.Add(InvalidCardinalityDiagnostic(returnType));
                    yield break;
                }
            }

            yield return new Binding(
                entries,
                metadataType,
                isRetrySupported);
        }

        private Diagnostic InvalidCardinalityDiagnostic(ITypeSymbol rawOutputTypeSymbol)
        {
            return Diagnostic.Create(
                DiagnosticDescriptors.InvalidCardinality,
                _parameter.Locations.FirstOrDefault(),
                new[] { rawOutputTypeSymbol.GetFullName() });
        }

        private static bool UnwrapAsyncEnumerable(
            ITypeSymbol typeSymbol,
            out DataType metadataType,
            out Diagnostic? diagnostic)
        {
            return UnwrapGenericCollectionInterface(typeSymbol,
                "IAsyncEnumerable",
                "System.Collections.Generic",
                out metadataType,
                out diagnostic);
        }

        private static bool UnwrapEnumerable(
            ITypeSymbol typeSymbol,
            out DataType metadataType,
            out Diagnostic? diagnostic)
        {
            return UnwrapGenericCollectionInterface(typeSymbol,
                "IEnumerable",
                "System.Collections.Generic",
                out metadataType,
                out diagnostic);
        }

        private static bool UnwrapGenericCollectionInterface(
            ITypeSymbol typeSymbol,
            string genericClassName,
            string genericClassNamespace,
            out DataType metadataType,
            out Diagnostic? diagnostic)
        {
            metadataType = DataType.Undefined;
            diagnostic = null;

            if (typeSymbol.Kind == SymbolKind.ArrayType
                && typeSymbol is IArrayTypeSymbol arraySymbol
                && ParsedType.TryParse(
                    arraySymbol.ElementType,
                    out var arrayInnerType,
                    out _,
                    out diagnostic))
            {
                metadataType = arrayInnerType?.GetGeneratorDataType() ?? DataType.Undefined;
                return arrayInnerType is not null;
            }

            if (typeSymbol is not INamedTypeSymbol namedSymbol)
            {
                return false;
            }

            ParsedType? type = null;
            foreach (var symbolInterface in new[] { namedSymbol }.Concat(namedSymbol.AllInterfaces))
            {
                if (symbolInterface.Name != genericClassName
                    || !symbolInterface.IsGenericType
                    || symbolInterface.TypeArguments.Length != 1
                    || symbolInterface.ContainingNamespace is null
                    || symbolInterface.ContainingNamespace.ToString() != genericClassNamespace)
                {
                    continue;
                }

                if (type is not null)
                {
                    diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.InvalidBindingType,
                        typeSymbol.Locations.FirstOrDefault(),
                        new[] { $"more than 1 implementation of {genericClassName}", "batched trigger" });
                    return false;
                }

                var innerType = symbolInterface.TypeArguments[0];
                if (!ParsedType.TryParse(
                    innerType,
                    out type,
                    out _,
                    out diagnostic))
                {
                    return false;
                }
            }

            metadataType = type?.GetGeneratorDataType() ?? DataType.Undefined;
            return type is not null;
        }

        private Diagnostic CreateMissingSymbol(string missingPart)
        {
            return Diagnostic.Create(
                DiagnosticDescriptors.SymbolNotFound,
                _parameter.Locations.FirstOrDefault() ?? _functionMethodSymbol.Locations.FirstOrDefault(),
                new[] { missingPart });
        }

        private bool ExtractIsBatchedTrigger(
            Dictionary<string, string> entries)
        {
            var isSupported = HasCardinalitySupport();
            if (!isSupported)
            {
                return false;
            }

            var isBatched = false;
            if (entries.TryGetValue(IsBatchedAttribute, out var isBatchedString))
            {
                entries.Remove(IsBatchedAttribute);
                if (!bool.TryParse(isBatchedString, out isBatched))
                {
                    _diagnostics.Add(CreateMissingSymbol("bool value"));
                }
            }

            entries[CardinalityAttribute] = isBatched ? "Many" : "One";

            return isBatched;
        }

        private bool HasCardinalitySupport()
        {
            return _attribute.AttributeClass is not null
                && _attribute.AttributeClass.AllInterfaces
                .Any(x => x.Name == "ISupportCardinality"
                    && x.ContainingNamespace is not null
                    && x.ContainingNamespace.ToString() == "Microsoft.Azure.Functions.Worker.Extensions.Abstractions");
        }

        private string GetShortType()
        {
            return _attribute.AttributeClass.Name
                .Replace("Attribute", string.Empty)
                .ToCammelCase();
        }

        private string? TryGetJsonValue(TypedConstant value)
        {
            if (value.Kind is TypedConstantKind.Primitive or TypedConstantKind.Type)
            {
                return value.Value.ToString();
            }

            if (value.Kind == TypedConstantKind.Array)
            {
                var values = value.Values.Select(x => TryGetJsonValue(x)).ToArray();
                return JsonSerializer.Serialize(values);
            }

            if (value.Kind == TypedConstantKind.Enum)
            {
                var enumValue = value.Type!.GetMembers()
                    .FirstOrDefault(m => m is IFieldSymbol field
                        && field.ConstantValue is object contantValue
                        && contantValue.Equals(value.Value));

                if (enumValue is not null)
                {
                    return enumValue.Name;
                }
            }

            _diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.InvalidBindingAttributeArgument,
                _parameter.Locations.FirstOrDefault() ?? _functionMethodSymbol.Locations.FirstOrDefault()));

            return null;
        }

        [DebuggerDisplay("{ToDebugInfo()}")]
        private sealed class Binding : IGenerateableBinding
        {
            private readonly Dictionary<string, string> _entries;

            public Binding(
                Dictionary<string, string> entries,
                DataType declaredType,
                bool isRetrySupported)
            {
                _entries = entries;
                DeclaredType = declaredType;
                IsRetrySupported = isRetrySupported;
            }

            public string BindingName => _entries.TryGetValue("name", out var name) ? name : "unknown";

            public BindingType BindingType => BindingType.Trigger;

            public bool IsParsable => true;

            public DataType DeclaredType { get; }
            public bool IsRetrySupported { get; }

            public string ToGeneratedBinding()
            {
                var properties = _entries
                    .Where(x => !_wellKnowAttributes.Contains(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value);

                var propertiesCode = properties.Count == 0
                    ? @"global::System.Collections.Immutable.ImmutableDictionary<string,string>.Empty"
                    : $$"""
                    new global::System.Collections.Generic.Dictionary<string, string>({{properties.Count}})
                    {
                        {{string.Join($",{Environment.NewLine}    ", properties.Select(AsDictionaryValue))}}
                    }
                    """;

                return $$"""
                    new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                        name: "{{_entries["name"]}}",
                        direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                        bindingType: "{{_entries[TypeAttribute]}}",
                        dataType: null /* TODO */,
                        properties: {{propertiesCode.Replace(Environment.NewLine, Environment.NewLine + "    ")}})
                    """;
            }

            private string AsDictionaryValue(KeyValuePair<string, string> pair)
            {
                return $$"""
                    { "{{pair.Key}}", "{{pair.Value}}" }
                    """;
            }

            public string ToRawBinding()
            {
                var json = JsonSerializer.Serialize(_entries);
                return json;
            }

            private string ToDebugInfo()
            {
                return $"Default {string.Join("; ", _entries.Select(x => $"{x.Key}-{x.Value}"))}";
            }
        }

        [DebuggerDisplay("{Name}->{Value}")]
        private record struct Entry
        {
            public Entry(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; }
            public string Value { get; }
        }
    }
}
