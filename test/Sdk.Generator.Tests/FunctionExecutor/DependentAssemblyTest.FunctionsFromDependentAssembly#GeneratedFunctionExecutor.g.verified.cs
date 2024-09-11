﻿//HintName: GeneratedFunctionExecutor.g.cs
// <auto-generated/>
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Context.Features;
using Microsoft.Azure.Functions.Worker.Invocation;
namespace TestProject
{
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class DirectFunctionExecutor : global::Microsoft.Azure.Functions.Worker.Invocation.IFunctionExecutor
    {
        private readonly global::Microsoft.Azure.Functions.Worker.IFunctionActivator _functionActivator;
        private readonly Dictionary<string, Type> types = new Dictionary<string, Type>()
        {
            { "MyCompany.MyHttpTriggers", Type.GetType("MyCompany.MyHttpTriggers, TestProject, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null") }
        };

        public DirectFunctionExecutor(global::Microsoft.Azure.Functions.Worker.IFunctionActivator functionActivator)
        {
            _functionActivator = functionActivator ?? throw new global::System.ArgumentNullException(nameof(functionActivator));
        }

        /// <inheritdoc/>
        public async global::System.Threading.Tasks.ValueTask ExecuteAsync(global::Microsoft.Azure.Functions.Worker.FunctionContext context)
        {
            var inputBindingFeature = context.Features.Get<global::Microsoft.Azure.Functions.Worker.Context.Features.IFunctionInputBindingFeature>();
            var inputBindingResult = await inputBindingFeature.BindFunctionInputAsync(context);
            var inputArguments = inputBindingResult.Values;

            if (string.Equals(context.FunctionDefinition.EntryPoint, "MyCompany.MyHttpTriggers.Foo", StringComparison.Ordinal))
            {
                var instanceType = types["MyCompany.MyHttpTriggers"];
                var i = _functionActivator.CreateInstance(instanceType, context) as global::MyCompany.MyHttpTriggers;
                context.GetInvocationResult().Value = i.Foo((global::Microsoft.Azure.Functions.Worker.Http.HttpRequestData)inputArguments[0], (global::Microsoft.Azure.Functions.Worker.FunctionContext)inputArguments[1]);
            }
        }
    }

    /// <summary>
    /// Extension methods to enable registration of the custom <see cref="IFunctionExecutor"/> implementation generated for the current worker.
    /// </summary>
    public static class FunctionExecutorHostBuilderExtensions
    {
        ///<summary>
        /// Configures an optimized function executor to the invocation pipeline.
        ///</summary>
        public static IHostBuilder ConfigureGeneratedFunctionExecutor(this IHostBuilder builder)
        {
            return builder.ConfigureServices(s => 
            {
                s.AddSingleton<global::Microsoft.Azure.Functions.Worker.Invocation.IFunctionExecutor, DirectFunctionExecutor>();
            });
        }
    }
}