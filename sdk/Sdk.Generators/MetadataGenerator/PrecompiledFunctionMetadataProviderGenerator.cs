using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Azure.Functions.Worker.Sdk.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.MetadataGenerator
{
    [Generator]
    public sealed class PrecompiledFunctionMetadataProviderGenerator : IIncrementalGenerator
    {
        private readonly IReadOnlyCollection<IPrecompiledFunctionMetadataEmiter> _emiters;

        public PrecompiledFunctionMetadataProviderGenerator()
        {
            _emiters = new IPrecompiledFunctionMetadataEmiter[]
            {
                new BindingDeclarationEmiter()
            };
        }

        public PrecompiledFunctionMetadataProviderGenerator(IReadOnlyCollection<IPrecompiledFunctionMetadataEmiter> emiters)
        {
            _emiters = emiters;
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var allModels = context
                .SyntaxProvider
                .ForAttributeWithMetadataName(
                    "Microsoft.Azure.Functions.Worker.FunctionAttribute",
                    static (node, ct) => true,
                    GetModel)
                .Where(x => x is not null);

            var model = allModels
                .Collect()
                .Combine(AnalyzerConfigurationProvider.Load(context));

            context.RegisterSourceOutput(
                model,
                (ctx, data) =>
                {
                    var (src, analyzer) = data;
                    if (!analyzer.IsRunningInAzureFunctionProject)
                    {
                        return;
                    }

                    if (!analyzer.ShouldExecuteGeneration)
                    {
                        return;
                    }

                    foreach (var diagnostic in src
                        .Where(x => x.Diagnostics.Count > 0)
                        .SelectMany(x => x.Diagnostics))
                    {
                        ctx.ReportDiagnostic(diagnostic);
                    }

                    var validModels = src
                        .Where(x => x.IsValid && x.Declaration is not null)
                        .Select(x => x.Declaration)
                        .ToArray();

                    foreach (var emitter in _emiters)
                    {
                        emitter.Emit(ctx, validModels, analyzer);
                    }
                });
        }

        private static Model GetModel(
            GeneratorAttributeSyntaxContext context,
            CancellationToken token)
        {
            var model = new Model();
            if (context.Attributes.Length != 1)
            {
                AddInconclusiveDiagnostic(context, model);
                return model;
            }

            var attribute = context.Attributes.First();
            if (context.TargetSymbol is not IMethodSymbol methodSymbol
                || context.TargetNode is not MethodDeclarationSyntax methodNode)
            {
                AddInconclusiveDiagnostic(context, model);
                return model;
            }

            var functionAttributeName = attribute.GetArgumentByConstructor(0);
            if (functionAttributeName is null || functionAttributeName.Value.IsNull)
            {
                model.Diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.SymbolNotFound,
                    context.TargetNode?.GetLocation() ?? Location.None,
                    "Name"));
                return model;
            }

            var parser = new BindingParser(methodSymbol, context.SemanticModel, token);
            var parsedBindings = parser.Load();
            foreach (var item in parser.Diagnostics)
            {
                model.Diagnostics.Add(item);
            }

            if (!parser.IsValid
                || functionAttributeName?.Value is null
                || !TryParseRetry(
                    model,
                    methodSymbol,
                    parsedBindings.All(x => x.IsRetrySupported),
                    out var retry))
            {
                return model;
            }

            model.Declaration = new FunctionDeclaration()
            {
                MethodName = methodSymbol.Name,
                ContainingTypeName = context.TargetSymbol.ContainingType.ToString(),
                FunctionName = functionAttributeName.Value.Value.ToString(),
                Retry = retry
            };

            model.Declaration.Bindings.AddRange(parsedBindings);

            return model;
        }

        private static bool TryParseRetry(
            Model model,
            IMethodSymbol methodSymbol,
            bool isBindingSupportsRetry,
            out RetryModel? retry)
        {
            void AddInvalidArgument(string parameter, string reason)
            {
                model.Diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidRetryArgument,
                    methodSymbol.Locations.FirstOrDefault(),
                    new[] { parameter, reason }));
            }

            retry = null;
            var retries = new List<RetryModel>();
            foreach (var attribute in methodSymbol.GetAttributes())
            {
                if (attribute.AttributeClass?.ContainingNamespace is null
                    || attribute.AttributeClass.ContainingNamespace.ToString() != "Microsoft.Azure.Functions.Worker")
                {
                    continue;
                }

                if (attribute.AttributeClass.Name == "ExponentialBackoffRetry")
                {
                    var maxRetryCountString = attribute.GetArgumentByConstructor(0)?.Value?.ToString();
                    var minimumIntervalString = attribute.GetArgumentByConstructor(1)?.Value?.ToString();
                    var maximumIntervalString = attribute.GetArgumentByConstructor(2)?.Value?.ToString();

                    if (string.IsNullOrWhiteSpace(maxRetryCountString)
                        || !int.TryParse(maxRetryCountString, NumberStyles.Any, CultureInfo.InvariantCulture, out var maxRetryCount)
                        || maxRetryCount <= 0)
                    {
                        AddInvalidArgument("maxRetryCount", "integer grater than 0");
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(minimumIntervalString)
                        || !TimeSpan.TryParse(minimumIntervalString, out var minimumInterval)
                        || minimumInterval.Ticks < 0)
                    {
                        AddInvalidArgument("minimumInterval", "valid positive TimeSpan");
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(maximumIntervalString)
                        || !TimeSpan.TryParse(maximumIntervalString, out var maximumInterval)
                        || maximumInterval.Ticks < 0)
                    {
                        AddInvalidArgument("maximumInterval", "valid positive TimeSpan");
                        return false;
                    }

                    if (minimumInterval > maximumInterval)
                    {
                        AddInvalidArgument("minimumInterval", "not be greater than the maximumInterval");
                        return false;
                    }

                    retries.Add(RetryModel.AsExponentialBackoff(maxRetryCount, minimumInterval, maximumInterval));
                }
                else if (attribute.AttributeClass.Name == "FixedDelayRetryAttribute")
                {
                    var maxRetryCountString = attribute.GetArgumentByConstructor(0)?.Value?.ToString();
                    var delayIntervalString = attribute.GetArgumentByConstructor(1)?.Value?.ToString();

                    if (string.IsNullOrWhiteSpace(maxRetryCountString)
                        || !int.TryParse(maxRetryCountString, NumberStyles.Any, CultureInfo.InvariantCulture, out var maxRetryCount)
                        || maxRetryCount <= 0)
                    {
                        AddInvalidArgument("maxRetryCount", "integer grater than 0");
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(delayIntervalString)
                        || !TimeSpan.TryParse(delayIntervalString, out var delayInterval)
                        || delayInterval.Ticks < 0)
                    {
                        AddInvalidArgument("minimumInterval", "valid positive TimeSpan");
                        return false;
                    }

                    retries.Add(RetryModel.AsFixedDelay(maxRetryCount, delayInterval));
                }
            }

            if (retries.Count > 1)
            {
                model.Diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidRetryArgument,
                    methodSymbol.Locations.FirstOrDefault(),
                    new[] { "retry attribute", "at most 1" }));

                return false;
            }

            if (!isBindingSupportsRetry
                && retries.Count > 0)
            {
                model.Diagnostics.Add(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidRetryOptions,
                    methodSymbol.Locations.FirstOrDefault()));

                return false;
            }

            retry = retries.FirstOrDefault();
            return true;
        }

        private static void AddInconclusiveDiagnostic(
            GeneratorAttributeSyntaxContext context,
            Model model)
        {
            model.Diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.InconclusiveAttribute,
                context.TargetNode?.GetLocation() ?? Location.None));
        }

        private record Model
        {
            public List<Diagnostic> Diagnostics { get; } = new();

            public FunctionDeclaration? Declaration { get; set; }

            public bool IsValid
                => !Diagnostics.Any(x => x.Severity >= DiagnosticSeverity.Error)
                && Declaration is not null;
        }
    }
}
