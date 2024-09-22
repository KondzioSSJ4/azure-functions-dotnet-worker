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
            Function0RawBindings.Add(@"{""name"":""$return"",""type"":""serviceBus"",""direction"":""Out"",""queueOrTopicName"":""outputQueue1"",""entityType"":""Queue"",""connection"":""ServiceBusConnection""}");
            Function0RawBindings.Add(@"{""name"":""message"",""type"":""serviceBusTrigger"",""direction"":""In"",""properties"":{""supportsDeferredBinding"":""True""},""queueName"":""queue1"",""connection"":""ServiceBusConnection"",""cardinality"":""One""}");

            var Function0 = new DefaultFunctionMetadata
            {
                Language = "dotnet-isolated",
                Name = "VoidResult",
                EntryPoint = "SampleApp.TestClass.VoidResult",
                RawBindings = Function0RawBindings,
                ScriptFile = "TestProject.dll"
            };
            metadataList.Add(Function0);
            var Function1RawBindings = new List<string>();
            Function1RawBindings.Add(@"{""name"":""$return"",""type"":""serviceBus"",""direction"":""Out"",""queueOrTopicName"":""outputQueue2"",""entityType"":""Queue"",""connection"":""ServiceBusConnection""}");
            Function1RawBindings.Add(@"{""name"":""message"",""type"":""serviceBusTrigger"",""direction"":""In"",""properties"":{""supportsDeferredBinding"":""True""},""queueName"":""queue2"",""connection"":""ServiceBusConnection"",""cardinality"":""One""}");

            var Function1 = new DefaultFunctionMetadata
            {
                Language = "dotnet-isolated",
                Name = "StringResult",
                EntryPoint = "SampleApp.TestClass.StringResult",
                RawBindings = Function1RawBindings,
                ScriptFile = "TestProject.dll"
            };
            metadataList.Add(Function1);
            var Function2RawBindings = new List<string>();
            Function2RawBindings.Add(@"{""name"":""$return"",""type"":""serviceBus"",""direction"":""Out"",""queueOrTopicName"":""outputQueue3"",""entityType"":""Queue"",""connection"":""ServiceBusConnection""}");
            Function2RawBindings.Add(@"{""name"":""message"",""type"":""serviceBusTrigger"",""direction"":""In"",""properties"":{""supportsDeferredBinding"":""True""},""queueName"":""queue3"",""connection"":""ServiceBusConnection"",""cardinality"":""One""}");

            var Function2 = new DefaultFunctionMetadata
            {
                Language = "dotnet-isolated",
                Name = "TaskResult",
                EntryPoint = "SampleApp.TestClass.TaskResult",
                RawBindings = Function2RawBindings,
                ScriptFile = "TestProject.dll"
            };
            metadataList.Add(Function2);
            var Function3RawBindings = new List<string>();
            Function3RawBindings.Add(@"{""name"":""$return"",""type"":""serviceBus"",""direction"":""Out"",""queueOrTopicName"":""outputQueue4"",""entityType"":""Queue"",""connection"":""ServiceBusConnection""}");
            Function3RawBindings.Add(@"{""name"":""message"",""type"":""serviceBusTrigger"",""direction"":""In"",""properties"":{""supportsDeferredBinding"":""True""},""queueName"":""queue4"",""connection"":""ServiceBusConnection"",""cardinality"":""One""}");
            Function3RawBindings.Add(@"{""name"":""OutputEvent1"",""type"":""serviceBus"",""direction"":""Out"",""queueOrTopicName"":""TopicOrQueueName1"",""entityType"":""Queue"",""connection"":""ServiceBusConnection""}");
            Function3RawBindings.Add(@"{""name"":""OutputEvent2"",""type"":""serviceBus"",""direction"":""Out"",""queueOrTopicName"":""TopicOrQueueName2"",""entityType"":""Queue"",""connection"":""ServiceBusConnection""}");

            var Function3 = new DefaultFunctionMetadata
            {
                Language = "dotnet-isolated",
                Name = "NestedClassResult",
                EntryPoint = "SampleApp.TestClass.NestedClassResult",
                RawBindings = Function3RawBindings,
                ScriptFile = "TestProject.dll"
            };
            metadataList.Add(Function3);
            var Function4RawBindings = new List<string>();
            Function4RawBindings.Add(@"{""name"":""$return"",""type"":""serviceBus"",""direction"":""Out"",""queueOrTopicName"":""outputQueue5"",""entityType"":""Queue"",""connection"":""ServiceBusConnection""}");
            Function4RawBindings.Add(@"{""name"":""message"",""type"":""serviceBusTrigger"",""direction"":""In"",""properties"":{""supportsDeferredBinding"":""True""},""queueName"":""queue5"",""connection"":""ServiceBusConnection"",""cardinality"":""One""}");
            Function4RawBindings.Add(@"{""name"":""OutputEvent1"",""type"":""serviceBus"",""direction"":""Out"",""queueOrTopicName"":""TopicOrQueueName1"",""entityType"":""Queue"",""connection"":""ServiceBusConnection""}");
            Function4RawBindings.Add(@"{""name"":""OutputEvent2"",""type"":""serviceBus"",""direction"":""Out"",""queueOrTopicName"":""TopicOrQueueName2"",""entityType"":""Queue"",""connection"":""ServiceBusConnection""}");

            var Function4 = new DefaultFunctionMetadata
            {
                Language = "dotnet-isolated",
                Name = "AsyncNestedClassResult",
                EntryPoint = "SampleApp.TestClass.AsyncNestedClassResult",
                RawBindings = Function4RawBindings,
                ScriptFile = "TestProject.dll"
            };
            metadataList.Add(Function4);

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