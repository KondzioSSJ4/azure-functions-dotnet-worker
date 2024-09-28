using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.MetadataGenerator
{
    public sealed class BindingDeclarationEmiter : IPrecompiledFunctionMetadataEmiter
    {
        internal const string ProviderName = "PrecompiledFunctionMetadataProvider";
        public const string AssemblyMetadataFile = "PrecompiledFunctionMetadataProviderGenerator.g.cs";

        public const string AggregatorFile = "PrecompiledFunctionMetadataStartup.g.cs";

        public void Emit(
            SourceProductionContext ctx,
            IReadOnlyCollection<FunctionDeclaration> src,
            AnalyzerConfigurationProvider analyzer)
        {
            if (src.Count == 0)
            {
                return;
            }

            EmitAssemblyMetadata(ctx, src, analyzer);
            EmitMetadataAggregator(ctx, analyzer);
        }

        private static void EmitMetadataAggregator(SourceProductionContext ctx, AnalyzerConfigurationProvider analyzer)
        {
            ctx.AddSource(
                            AggregatorFile,
                            $$"""
                using System.Linq;
                using System.Collections.Immutable;
                using Microsoft.Extensions.Hosting;
                using Microsoft.Extensions.DependencyInjection;

                namespace AzureFunctionInternals.{{analyzer.AssemblyName}}
                {
                    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
                    internal sealed class PrecompiledFunctionMetadataAggregator : global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider
                    {
                        private readonly global::System.Collections.Generic.IEnumerable<
                                global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IAssemblyTypeProvider<
                                    global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider>> _providers;

                        public PrecompiledFunctionMetadataAggregator(
                            global::System.Collections.Generic.IEnumerable<
                                global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IAssemblyTypeProvider<
                                    global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider>> providers)
                        {
                            _providers = providers;
                        }

                        public async global::System.Threading.Tasks.Task<
                            global::System.Collections.Immutable.ImmutableArray<
                                global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>> GetFunctionMetadataAsync(
                            string directory)
                        {
                            var copy = _providers.Where(x => x.Value is not null).Select(x => x.Value).ToArray();
                            if (copy.Length == 1)
                            {
                                return await copy[0].GetFunctionMetadataAsync(directory);
                            }

                            var results = await global::System.Threading.Tasks.WhenAll(
                                copy.Select(x => x.GetFunctionMetadataAsync(directory)));

                            return results.ToImmutableArray();
                        }
                    }

                    /// <summary>
                    /// Auto startup class to register the custom <see cref="global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider"/> implementation generated for the current worker.
                    /// </summary>
                    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
                    public sealed class PrecompiledFunctionMetadataProviderAutoStartup : global::Microsoft.Azure.Functions.Worker.IAutoConfigureStartup
                    {
                        /// <summary>
                        /// Configures the <see cref="global::Microsoft.Extensions.Hosting.IHostBuilder"/> to use the custom <see cref="IFunctionMetadataProvider"/> implementation generated for the current worker.
                        /// </summary>
                        /// <param name="hostBuilder">The <see cref="global::Microsoft.Extensions.Hosting.IHostBuilder"/> instance to use for service registration.</param>
                        public void Configure(
                            global::Microsoft.Extensions.Hosting.IHostBuilder builder)
                        {
                            builder.ConfigureServices(s =>
                            {
                                s.AddSingleton<global::AzureFunctionInternals.{{analyzer.AssemblyName}}.{{BindingDeclarationEmiter.ProviderName}}>();

                                s.AddSingleton<
                                    global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IAssemblyTypeProvider<
                                        global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider>,
                                    global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.AssemblyTypeProvider<
                                        global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider,
                                        global::AzureFunctionInternals.{{analyzer.AssemblyName}}.{{BindingDeclarationEmiter.ProviderName}}>>();

                                s.AddSingleton<
                                    global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider,
                                    global::AzureFunctionInternals.{{analyzer.AssemblyName}}.PrecompiledFunctionMetadataAggregator>();
                            });
                        }
                    }
                }
                """);
        }

        private static void EmitAssemblyMetadata(SourceProductionContext ctx, IReadOnlyCollection<FunctionDeclaration> src, AnalyzerConfigurationProvider analyzer)
        {
            ctx.AddSource(
                            AssemblyMetadataFile,
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
                    public sealed class {{ProviderName}} : global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider
                    {
                        public global::System.Threading.Tasks.Task<global::System.Collections.Immutable.ImmutableArray<global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>> GetFunctionMetadataAsync(
                            string directory)
                        {
                            var results = global::System.Collections.Immutable.ImmutableArray.Create<global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>(new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata[]
                            {
                {{BuildMetadata(src, analyzer)}}
                            });

                            return global::System.Threading.Tasks.Task.FromResult<
                                global::System.Collections.Immutable.ImmutableArray<
                                    global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>>(results);
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
                        functionId: "{{HashFunctionId(function.FunctionName, analyzer.AssemblyFileName, function.MethodName)}}",
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
                    foreach (var c in value)
                    {
                        hash = hash * 31 + c;
                    }

                    return hash;
                }
            }

            unchecked
            {
                var atLeastOnePresent = false;
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
    }
}
