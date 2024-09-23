using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Azure.Functions.Worker.Sdk.Generators.Extensions;
using Microsoft.Azure.Functions.Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator.BindingGenerator;
using Microsoft.CodeAnalysis;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator
{
    internal sealed class BindingParser
    {
        private const string MethodResultName = Constants.FunctionMetadataBindingProps.ReturnBindingName;

        private readonly IMethodSymbol _functionMethodSymbol;
        private readonly SemanticModel _semanticModel;
        private readonly CancellationToken _cancellationToken;
        private readonly List<Diagnostic> _diagnostics = new();

        public IReadOnlyCollection<Diagnostic> Diagnostics => _diagnostics;
        public bool IsValid { get; private set; } = true;

        public BindingParser(
            IMethodSymbol functionMethodSymbol,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            _functionMethodSymbol = functionMethodSymbol;
            _semanticModel = semanticModel;
            _cancellationToken = cancellationToken;
        }

        public IReadOnlyCollection<IGenerateableBinding> Load()
        {
            var bindings = LoadTriggerBindings()
                .Concat(LoadOutputBindings())
                .ToArray();

            if (Diagnostics.Count(x => x.Severity >= DiagnosticSeverity.Error) > 0)
            {
                return Array.Empty<IGenerateableBinding>();
            }

            var triggerBindingCount = bindings.Count(x => x.BindingType == BindingType.Trigger);
            if (triggerBindingCount != 1)
            {
                AddDiagnostic(DiagnosticDescriptors.InvalidInputTriggerCount, new object[] { triggerBindingCount });
                return Array.Empty<IGenerateableBinding>();
            }

            return bindings;

        }

        private IEnumerable<IGenerateableBinding> LoadOutputBindings()
        {
            if (IsAsyncVoid())
            {
                AddDiagnostic(DiagnosticDescriptors.AsyncVoidIsNotAllowed);
                yield break;
            }

            if (!ParsedType.TryParse(
                _functionMethodSymbol.ReturnType,
                out var outputType,
                out var rawOutputSymbol,
                out var parseDiagnostic))
            {
                _diagnostics.Add(parseDiagnostic);
                yield break;
            }

            var hasNestedOutputs = false;
            foreach (var property in GetInnerProperties(rawOutputSymbol))
            {
                var innerModelBindings = LoadBindingsByAttributes(property, property.Name).ToArray();
                if (innerModelBindings.Length == 0)
                {
                    continue;
                }

                if (outputType.IsAsyncEnumerable || outputType.IsEnumerable)
                {
                    _diagnostics.Add(Diagnostic.Create(
                        DiagnosticDescriptors.InvalidBindingType,
                        GetDefaultLocation(),
                        new[] { "IAsyncEnumerable or IEnumerable", "multi-binding return" }));
                    yield break;
                }

                hasNestedOutputs = true;
                foreach (var bindingPair in innerModelBindings)
                {
                    yield return bindingPair.Binding;
                }
            }

            var methodAttributes = LoadBindingsByAttributes(_functionMethodSymbol, MethodResultName).ToArray();
            if (methodAttributes.Length > 0
                && hasNestedOutputs)
            {
                _diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.InconclusiveOutputBinding,
                    _functionMethodSymbol.Locations.FirstOrDefault()));
            }

            if (hasNestedOutputs)
            {
                yield break;
            }

            foreach (var returnTypeBindingPair in methodAttributes)
            {
                yield return returnTypeBindingPair.Binding;
            }
        }

        private IEnumerable<(IGenerateableBinding Binding, AttributeData Attribute)> LoadBindingsByAttributes(
            ISymbol symbol,
            string referenceName)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                _cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(attribute?.AttributeClass?.Name)
                    || !IsOutputAttribute(attribute!.AttributeClass))
                {
                    continue;
                }

                IGenerateableBindingGenerator? bindingGenerator = attribute.AttributeClass!.Name switch
                {
                    //"ServiceBusOutputAttribute" => new ServiceBusOutputParser(symbol, attribute, referenceName),
                    _ => throw new NotImplementedException()
                };

                var bindings = bindingGenerator?.Generate(_cancellationToken).ToArray() ?? Array.Empty<IGenerateableBinding>();

                if (bindingGenerator?.ParsedDiagnostics?.Count > 0)
                {
                    _diagnostics.AddRange(bindingGenerator.ParsedDiagnostics);
                }

                foreach (var binding in bindings)
                {
                    yield return (binding, attribute);
                }
            }
        }

        private static bool IsOutputAttribute(INamedTypeSymbol? attributeClass)
        {
            return attributeClass is not null
                && attributeClass.IsInheritedFrom(
                    "Microsoft.Azure.Functions.Worker.Extensions.Abstractions",
                    "OutputBindingAttribute");
        }

        private IEnumerable<IPropertySymbol> GetInnerProperties(ITypeSymbol rawOutputSymbol)
        {
            return rawOutputSymbol
                .GetMembers()
                .Where(m => m.Kind == SymbolKind.Property)
                .OfType<IPropertySymbol>();
        }

        private bool IsAsyncVoid()
        {
            return _functionMethodSymbol.ReturnsVoid
                && _functionMethodSymbol.IsAsync;
        }

        private IEnumerable<IGenerateableBinding> LoadTriggerBindings()
        {
            foreach (var parameter in _functionMethodSymbol.Parameters)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                var attributes = parameter.GetAttributes();
                if (attributes.Length == 0)
                {
                    continue;
                }

                foreach (var attribute in attributes)
                {
                    var attributeName = attribute.AttributeClass?.Name;
                    if (string.IsNullOrWhiteSpace(attributeName))
                    {
                        continue;
                    }

                    IGenerateableBindingGenerator bindingGenerator = attributeName switch
                    {
                        //"HttpTriggerAttribute" => new BindingGenerator.Http.HttpTriggerParser(_functionMethodSymbol, attribute, parameter),
                        _ => new BindingGenerator.Defaults.DefaultTriggerParser(_functionMethodSymbol, attribute, parameter)
                    };

                    var bindings = bindingGenerator?.Generate(_cancellationToken).ToArray() ?? Array.Empty<IGenerateableBinding>();

                    if (bindingGenerator?.ParsedDiagnostics?.Count > 0)
                    {
                        _diagnostics.AddRange(bindingGenerator.ParsedDiagnostics);
                    }

                    foreach (var result in bindings)
                    {
                        yield return result;
                    }
                }
            }
        }

        private void AddDiagnostic(
            DiagnosticDescriptor descriptor,
            object?[]? parameters = null)
        {
            _diagnostics.Add(Diagnostic.Create(
                descriptor,
                GetDefaultLocation(),
                parameters));
        }

        private Location GetDefaultLocation()
            => _functionMethodSymbol.Locations.FirstOrDefault()
            ?? Location.None;
    }
}
