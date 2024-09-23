using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator
{
    public sealed class BindingDeclarationEmiter : IPrecompiledFunctionMetadataEmiter
    {
        public void Emit(
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
