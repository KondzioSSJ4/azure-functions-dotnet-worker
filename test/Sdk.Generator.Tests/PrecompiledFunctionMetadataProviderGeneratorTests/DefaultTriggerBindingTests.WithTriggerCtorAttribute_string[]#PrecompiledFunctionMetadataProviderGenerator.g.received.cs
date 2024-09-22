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
               new DefaultFunctionMetadata
               {
                   Language = "dotnet-isolated",
                   Name = "Run",
                   EntryPoint = "Run",
                   ScriptFile = "TestProject.dll",
                   RawBindings = new List<string>(1)
                   {
                       @"{""name"":""value"",""type"":""customBindingTrigger"",""direction"":""In"",""input"":""[\u0022SomeStringValue\u0022]""}"
                   },
                   Retry = default // TODO
               }                

            });
        }
    }
}