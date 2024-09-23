using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator.BindingGenerator.Defaults
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
        private readonly AttributeData _attribute;
        private readonly IParameterSymbol _parameter;

        public DefaultTriggerParser(
            IMethodSymbol functionMethodSymbol,
            AttributeData attribute,
            IParameterSymbol parameter)
        {
            _functionMethodSymbol = functionMethodSymbol;
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

            for (int i = 0; i < _attribute.ConstructorArguments.Length; i++)
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

            var isBatched = ExtractIsBatchedTrigger(entries);
            if (isBatched
                && !declaredType.IsEnumerable
                && !declaredType.IsAsyncEnumerable)
            {
                _diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidCardinality,
                    _parameter.Locations.FirstOrDefault(),
                    new[] { declaredType.FullType }));
                yield break;
            }

            var isRetrySupported = _attribute.IsRetrySupported();

            ParsedType? rawOutputType = null;
            if (isBatched
                && (!ParsedType.TryParse(
                    rawOutputTypeSymbol,
                    out rawOutputType,
                    out _,
                    out diagnostic)
                    || declaredType is null))
            {
                _diagnostics.Add(diagnostic ?? CreateMissingSymbol(TypeAttribute));
                yield break;
            }

            yield return new Binding(
                entries,
                declaredType,
                rawOutputType ?? declaredType,
                isRetrySupported);
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
                ParsedType declaredType,
                ParsedType rawType,
                bool isRetrySupported)
            {
                _entries = entries;
                DeclaredType = declaredType;
                RawType = rawType;
                IsRetrySupported = isRetrySupported;
            }

            public string BindingName => _entries.TryGetValue("name", out var name) ? name : "unknown";

            public BindingType BindingType => BindingType.Trigger;

            public bool IsParsable => true;

            public ParsedType DeclaredType { get; }
            public ParsedType RawType { get; }
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
                        direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.In,
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
