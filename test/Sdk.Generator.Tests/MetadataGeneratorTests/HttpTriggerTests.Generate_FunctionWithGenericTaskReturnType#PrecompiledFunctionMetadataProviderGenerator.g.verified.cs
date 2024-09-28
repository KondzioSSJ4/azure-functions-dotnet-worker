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
                   functionId: "214064770",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "FunctionName",
                   entryPoint: "Http",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""myReq"",""type"":""httpTrigger"",""direction"":""In"",""authLevel"":""Admin"",""methods"":""[\u0022get\u0022,\u0022Post\u0022]"",""route"":""/api2""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "myReq",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: new global::System.Collections.Generic.Dictionary<string, string>(3)
                          {
                              { "authLevel", "Admin" },
                              { "methods", "["get","Post"]" },
                              { "route", "/api2" }
                          }) 
                   })                

            });

            return global::System.Threading.Tasks.Task.FromResult<
                global::System.Collections.Immutable.ImmutableArray<
                    global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>>(results);
        }
    }
}