//HintName: PrecompiledFunctionMetadataProviderGenerator.g.cs
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Core.FunctionMetadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AzureFunctionInternals.TestProject
{
    public sealed class PrecompiledFunctionMetadataProvider : global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider
    {
        public global::System.Threading.Tasks.Task<global::System.Collections.Immutable.ImmutableArray<global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>> GetFunctionMetadataAsync(
            string directory)
        {
            var results = global::System.Collections.Immutable.ImmutableArray.Create<global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>(new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata[]
            {
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "2882587761",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "Run",
                   entryPoint: "Run",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""value"",""type"":""serviceBusTrigger"",""direction"":""In"",""queueName"":""queueName"",""cardinality"":""Many""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "value",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "serviceBusTrigger",
                          dataType: null /* TODO */,
                          properties: new global::System.Collections.Generic.Dictionary<string, string>(2)
                          {
                              { "queueName", "queueName" },
                              { "cardinality", "Many" }
                          }) 
                   })                

            });

            return global::System.Threading.Tasks.Task.FromResult<
                global::System.Collections.Immutable.ImmutableArray<
                    global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>>(results);
        }
    }
}