﻿//HintName: GeneratedFunctionMetadataProvider.g.cs
// <auto-generated/>
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Core.FunctionMetadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestProject
{
    /// <summary>
    /// Custom <see cref="IFunctionMetadataProvider"/> implementation that returns function metadata definitions for the current worker."/>
    /// </summary>
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class GeneratedFunctionMetadataProvider : IFunctionMetadataProvider
    {
        /// <inheritdoc/>
        public Task<ImmutableArray<IFunctionMetadata>> GetFunctionMetadataAsync(string directory)
        {
            var metadataList = new List<IFunctionMetadata>();
            var Function0RawBindings = new List<string>();
            Function0RawBindings.Add(@"{""name"":""req"",""type"":""httpTrigger"",""direction"":""In"",""authLevel"":""Anonymous"",""methods"":[""get"",""post""]}");
            Function0RawBindings.Add(@"{""name"":""$return"",""type"":""http"",""direction"":""Out""}");

            var Function0 = new DefaultFunctionMetadata
            {
                Language = "dotnet-isolated",
                Name = "HttpTriggerSimple",
                EntryPoint = "FunctionApp.HttpTriggerSimple.Run",
                RawBindings = Function0RawBindings,
                ScriptFile = "TestProject.dll"
            };
            metadataList.Add(Function0);

            return Task.FromResult(metadataList.ToImmutableArray());
        }
    }

    /// <summary>
    /// Extension methods to enable registration of the custom <see cref="IFunctionMetadataProvider"/> implementation generated for the current worker.
    /// </summary>
    public static class WorkerHostBuilderFunctionMetadataProviderExtension
    {
        ///<summary>
        /// Adds the GeneratedFunctionMetadataProvider to the service collection.
        /// During initialization, the worker will return generated function metadata instead of relying on the Azure Functions host for function indexing.
        ///</summary>
        public static IHostBuilder ConfigureGeneratedFunctionMetadataProvider(this IHostBuilder builder)
        {
            builder.ConfigureServices(s => 
            {
                s.AddSingleton<IFunctionMetadataProvider, GeneratedFunctionMetadataProvider>();
            });
            return builder;
        }
    }
}