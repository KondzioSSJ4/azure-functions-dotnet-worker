using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.FunctionActivator
{
    [Generator]
    public sealed class FunctionActivatorGenerator : IIncrementalGenerator
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
                .Combine(context.AnalyzerConfigOptionsProvider);

            context.RegisterSourceOutput(
                model,
                static (ctx, data) =>
                {
                    var (src, analyzer) = data;

                    if (!analyzer.IsRunningInAzureFunctionProject())
                    {
                        return;
                    }

                    if (!analyzer.ShouldExecuteGeneration())
                    {
                        return;
                    }

                    foreach (var diagnostic in src
                        .Where(x => x.Diagnostics.Count > 0)
                        .SelectMany(x => x.Diagnostics))
                    {
                        ctx.ReportDiagnostic(diagnostic);
                    }

                    var uniqueClasses = src
                    .Where(x => x.IsValid)
                    .Select(x => x.Declaration.ClassWithNamespace)
                    .Distinct()
                    .ToArray();

                    if (uniqueClasses.Length == 0)
                    {
                        return;
                    }

                    var namespaceString = GetNamespace(analyzer);

                    ctx.AddSource(
                        "GeneratedFunctionActivator.g.cs",
                        $$"""
                        using System;
                        using Microsoft.Azure.Functions.Worker;
                        using Microsoft.Extensions.DependencyInjection;
                        using Microsoft.Extensions.Hosting;

                        namespace {{namespaceString}}
                        {
                            internal sealed class GeneratedFunctionActivator : IFunctionActivator
                            {
                                private readonly IServiceProvider _provider;
                        
                                public GeneratedFunctionActivator(IServiceProvider provider)
                                {
                                    _provider = provider;
                                }
                        
                                public object? CreateInstance(Type instanceType, FunctionContext context)
                                {
                                    if (instanceType is null)
                                    {
                                        throw new ArgumentNullException(nameof(instanceType));
                                    }
                        
                                    if (context is null)
                                    {
                                        throw new ArgumentNullException(nameof(context));
                                    }
                        
                                    return _provider.GetService(instanceType)
                                        ?? ActivatorUtilities.CreateInstance(context.InstanceServices, instanceType, Array.Empty<object>());
                                }
                            }

                            /// <summary>
                            /// Extension methods to enable registration of the custom <see cref="IFunctionActivator"/> implementation generated for the current worker.
                            /// </summary>
                            internal static class InjectableFunctionsExtensions
                            {
                                ///<summary>
                                /// Configures an optimized function activator to the invocation pipeline.
                                ///</summary>
                                public static IHostBuilder ConfigureGeneratedFunctionActivator(this IHostBuilder builder)
                                {
                                    return builder.ConfigureServices(s => 
                                    {
                                        s{{BuildInjectionOfFunctionClasses(uniqueClasses)}};

                                        s.AddSingleton<global::Microsoft.Azure.Functions.Worker.IFunctionActivator, global::{{namespaceString}}.GeneratedFunctionActivator>();
                                    });
                                }
                            }


                            /// <summary>
                            /// Auto startup class to register the custom <see cref="IFunctionActivator"/> implementation generated for the current worker.
                            /// </summary>
                            [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
                            public sealed class InjectableFunctionsActivator : global::Microsoft.Azure.Functions.Worker.IAutoConfigureStartup
                            {
                                /// <summary>
                                /// Configures the <see cref="IHostBuilder"/> to use the custom <see cref="IFunctionActivator"/> implementation generated for the current worker.
                                /// </summary>
                                /// <param name="hostBuilder">The <see cref="IHostBuilder"/> instance to use for service registration.</param>
                                public void Configure(IHostBuilder hostBuilder)
                                {
                                    hostBuilder.ConfigureGeneratedFunctionActivator();
                                }
                            }
                        }
                        """);
                });
        }

        private static string BuildInjectionOfFunctionClasses(IReadOnlyCollection<string> uniqueClasses)
        {
            var builder = new StringBuilder(uniqueClasses.Count * 3);
            foreach (var classFullName in uniqueClasses)
            {
                builder
                    .Append("\n                .AddTransient<global::")
                    .Append(classFullName)
                    .Append(">()");
            }

            return builder.ToString();
        }

        private static string GetNamespace(AnalyzerConfigOptionsProvider analyzer)
        {
            analyzer.GlobalOptions.TryGetValue(
                Constants.BuildProperties.GeneratedCodeNamespace,
                out var namespaceValue);

            return string.IsNullOrWhiteSpace(namespaceValue)
                ? "SourceGenerated"
                : namespaceValue!;
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
            if (context.TargetSymbol is not IMethodSymbol methodSymbol)
            {
                AddInconclusiveDiagnostic(context, model);
                return model;
            }

            if (!methodSymbol.ContainingSymbol.IsStatic
                && !methodSymbol.IsStatic)
            {
                if (methodSymbol.ContainingSymbol is INamedTypeSymbol classSymbol)
                {
                    var allowedConstructors = classSymbol.IsRecord ? 2 : 1;
                    if (classSymbol.InstanceConstructors.Length > allowedConstructors)
                    {
                        model.Diagnostics.Add(Diagnostic
                            .Create(
                                DiagnosticDescriptors.InconclusiveCtor,
                                context.TargetNode?.GetLocation() ?? Location.None));
                        return model;
                    }
                }

                model.Declaration = new()
                {
                    ClassWithNamespace = methodSymbol.ContainingSymbol.ToString()
                };
            }

            return model;
        }

        private static void AddInconclusiveDiagnostic(GeneratorAttributeSyntaxContext context, Model model)
        {
            model.Diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.InconclusiveAttribute,
                context.TargetNode?.GetLocation() ?? Location.None));
        }

        private record FunctionDeclaration
        {
            public string ClassWithNamespace { get; internal set; }
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
