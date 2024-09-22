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
    public sealed class PrecompiledFunctionMetadataProvider : IFunctionMetadataProvider
    {
        public Task<ImmutableArray<IFunctionMetadata>> GetFunctionMetadataAsync(string directory)
        {
            return ImmutableArray.Create<IFunctionMetadata>(new IFunctionMetadata[]
            {
               new SourceGeneratedFunctionMetadata(
                   functionId: 225417419,
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithMethodsOnly",
                   entryPoint: "WithMethodsOnly",
                   scriptFile: "TestProject.dll",
                   retry: null, /* TODO */
                   rawBindings: new List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In"",""methods"":[""get"",""post""]}"
                   },
                   generatedBindings: new List<IGeneratedBinding>(1)
                   { 
                       Method: get;post, AuthLevel:  
                   }),
               new SourceGeneratedFunctionMetadata(
                   functionId: 319677613,
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithAuthMethodOnly",
                   entryPoint: "WithAuthMethodOnly",
                   scriptFile: "TestProject.dll",
                   retry: null, /* TODO */
                   rawBindings: new List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In"",""authLevel"":""Function""}"
                   },
                   generatedBindings: new List<IGeneratedBinding>(1)
                   { 
                       Method: , AuthLevel: 2 
                   }),
               new SourceGeneratedFunctionMetadata(
                   functionId: 2714009341,
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithAuthLevelAndMethods",
                   entryPoint: "WithAuthLevelAndMethods",
                   scriptFile: "TestProject.dll",
                   retry: null, /* TODO */
                   rawBindings: new List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In"",""authLevel"":""Function"",""methods"":[""get""]}"
                   },
                   generatedBindings: new List<IGeneratedBinding>(1)
                   { 
                       Method: get, AuthLevel: 2 
                   }),
               new SourceGeneratedFunctionMetadata(
                   functionId: 295470615,
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithAuthLevelAndMethodsAndRoutes",
                   entryPoint: "WithAuthLevelAndMethodsAndRoutes",
                   scriptFile: "TestProject.dll",
                   retry: null, /* TODO */
                   rawBindings: new List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In"",""authLevel"":""Function"",""methods"":[""get""],""route"":""route""}"
                   },
                   generatedBindings: new List<IGeneratedBinding>(1)
                   { 
                       Method: get, AuthLevel: 2 
                   }),

            });
        }
    }
}