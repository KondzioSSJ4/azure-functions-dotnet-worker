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
                   functionId: 2882587761,
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "Run",
                   entryPoint: "Run",
                   scriptFile: "TestProject.dll",
                   retry: new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultRetryOptions()
                   {
                       MaxRetryCount = 1,
                       DelayInterval = new global::System.TimeSpan(0, 0, 0, 10, 0)
                   },
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""timerInfo"",""type"":""timerTrigger"",""direction"":""In"",""schedule"":""0 */5 * * * *""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: timerInfo,
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.In,
                          bindingType: "timerTrigger",
                          dataType: null /* TODO */,
                          properties: new global::System.Collections.Generic.Dictionary<string, string>(1)
                          {
                              { "schedule", "0 */5 * * * *" }
                          }) 
                   })                

            });

            return global::System.Threading.Tasks.Task<global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>(results);
        }
    }
}