using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Azure.Functions.Worker.Sdk.Generators.Extensions;
using Microsoft.Azure.Functions.Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator.BindingGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator
{
    [Generator]
    public class PrecompiledFunctionMetadataProviderGenerator : IIncrementalGenerator
    {
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
                static (ctx, data) =>
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

                    EmitBindingDeclarations(ctx, validModels, analyzer);
                });
        }

        private static void EmitBindingDeclarations(
            SourceProductionContext ctx,
            IReadOnlyCollection<FunctionDeclaration> src,
            AnalyzerConfigurationProvider analyzer)
        {
            if (src.Count == 0)
            {
                return;
            }

            ctx.AddSource(
                "PrecompiledFunctionMetadataProviderGenerator.g.cs",
                $$"""
                using System;
                using System.Collections.Generic;
                using System.Collections.Immutable;
                using System.Text.Json;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Core.FunctionMetadata;
                using Microsoft.Extensions.DependencyInjection;
                using Microsoft.Extensions.Hosting;

                namespace AzureFunctionInternals.{{analyzer.AssemblyName}}
                {
                    public sealed class PrecompiledFunctionMetadataProvider : global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider
                    {
                        public global::System.Threading.Tasks.Task<global::System.Collections.Immutable.ImmutableArray<global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>> GetFunctionMetadataAsync(
                            string directory)
                        {
                            var results = global::System.Collections.Immutable.ImmutableArray.Create<global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>(new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata[]
                            {
                {{BuildMetadata(src, analyzer)}}
                            });

                            return global::System.Threading.Tasks.Task<global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>(results);
                        }
                    }
                }
                """);
        }

        private static string BuildMetadata(
            IReadOnlyCollection<FunctionDeclaration> allFunctions,
            AnalyzerConfigurationProvider analyzer)
        {
            const string Indend = "   ";
            const string MetadataIndend = "                ";
            const string NewLineIndended = $"\r\n{MetadataIndend}";
            const string InnerValueIndend = NewLineIndended + Indend;
            var builder = new StringBuilder();

            foreach (var function in allFunctions)
            {
                var retry = function.Retry?.Code.Replace(Environment.NewLine, InnerValueIndend) ?? "null";

                if (IsAllBindingsSupportedPrecompilation(function))
                {
                    builder.Append($$"""
                    new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                        functionId: {{HashFunctionId(function.FunctionName, analyzer.AssemblyFileName, function.MethodName)}},
                        isProxy: false,
                        language: "dotnet-isolated",
                        managedDependencyEnabled: false,
                        name: "{{function.FunctionName}}",
                        entryPoint: "{{function.MethodName}}",
                        scriptFile: "{{analyzer.AssemblyFileName}}",
                        retry: {{retry}},
                        rawBindings: new global::System.Collections.Generic.List<string>({{function.Bindings.Count}})
                        { 
                            {{string.Join(NewLineIndended, GetRawBindings(function))}}
                        },
                        generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>({{function.Bindings.Count}})
                        { 
                            {{string.Join(NewLineIndended, function.Bindings.Select(b => b.ToGeneratedBinding().Replace(Environment.NewLine, InnerValueIndend + Indend)))}} 
                        })
     """);
                }
                else
                {
                    builder.Append($$"""
                    new DefaultFunctionMetadata
                    {
                        Language = "dotnet-isolated",
                        Name = "{{function.FunctionName}}",
                        EntryPoint = "{{function.MethodName}}",
                        ScriptFile = "{{analyzer.AssemblyFileName}}",
                        RawBindings = new global::System.Collections.Generic.List<string>({{function.Bindings.Count}})
                        {
                            {{string.Join(NewLineIndended, GetRawBindings(function))}}
                        },
                        Retry = {{retry}}
                    }
     """);
                }

                builder.Append(MetadataIndend).Append(",").AppendLine();
            }

            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - Environment.NewLine.Length - 1, 1);
            }

            return builder.ToString();
        }

        private static bool IsAllBindingsSupportedPrecompilation(FunctionDeclaration function)
        {
            return function.Bindings.All(x => x.IsParsable);
        }

        private static IEnumerable<string> GetRawBindings(FunctionDeclaration function)
        {
            foreach (var item in function.Bindings)
            {
                yield return $@"@""{item.ToRawBinding().Replace(@"""", @"""""")}""";
            }
        }

        private static string? HashFunctionId(
            string functionName,
            string scriptFile,
            string entryPoint)
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

                if (functionName is not null)
                {
                    atLeastOnePresent = true;
                    hash = hash * 31 + GetStableHash(functionName);
                }

                if (scriptFile is not null)
                {
                    atLeastOnePresent = true;
                    hash = hash * 31 + GetStableHash(scriptFile);
                }

                if (entryPoint is not null)
                {
                    atLeastOnePresent = true;
                    hash = hash * 31 + GetStableHash(entryPoint);
                }

                return atLeastOnePresent ? hash.ToString() : null;
            }
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

        private record FunctionDeclaration
        {
            public string FunctionName { get; internal set; }
            public string MethodName { get; set; }
            public string ContainingTypeName { get; set; }
            public List<IGenerateableBinding> Bindings { get; } = new();
            public RetryModel? Retry { get; set; }
            public IEnumerable<string> BindingsText => Bindings.Select(x => x.ToString());

        }

        [DebuggerDisplay("{Info}")]
        private record RetryModel
        {
            public string Code { get; }
            public string Info { get; }

            private RetryModel(
                string code,
                string info)
            {
                Code = code;
                Info = info;
            }

            public static RetryModel AsExponentialBackoff(
                int maxRetryCount,
                TimeSpan minimumInterval,
                TimeSpan maximumInterval)
            {
                var code = $$"""
                    new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultRetryOptions()
                    {
                        MaxRetryCount = {{maxRetryCount}},
                        MinimumInterval = {{ToCode(minimumInterval)}},
                        MaximumInterval = {{ToCode(maximumInterval)}}
                    }
                    """;

                var info = $"ExponentialBackoff {maxRetryCount}, {minimumInterval} to {maximumInterval}";

                return new RetryModel(code, info);
            }

            public static RetryModel AsFixedDelay(
                int maxRetryCount,
                TimeSpan delayInterval)
            {
                var code = $$"""
                    new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultRetryOptions()
                    {
                        MaxRetryCount = {{maxRetryCount}},
                        DelayInterval = {{ToCode(delayInterval)}}
                    }
                    """;

                var info = $"FixedDelay {maxRetryCount}, {delayInterval}";

                return new RetryModel(code, info);
            }

            private static string ToCode(TimeSpan value)
            {
                return $"new global::System.TimeSpan({value.Days}, {value.Hours}, {value.Minutes}, {value.Seconds}, {value.Milliseconds})";
            }
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
