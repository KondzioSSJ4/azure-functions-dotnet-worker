//HintName: GeneratedFunctionActivator.g.cs
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestProject
{
    internal sealed class GeneratedFunctionActivator : IFunctionActivator
    {
        private readonly IServiceProvider provider;

        public GeneratedFunctionActivator(IServiceProvider provider)
        {
            _provider = provider;
        }

        public object? CreateInstance(Type instanceType, FunctionContext context)
        {
            if (instanceType is null)
            {
                throw new ArgumentNullException(nameof(instanceType));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return provider.GetService(instanceType)
                ?? ActivatorUtilities.CreateInstance(context.InstanceServices, instanceType, Array.Empty<object>());
        }
    }

    /// <summary>
    /// Extension methods to enable registration of the custom <see cref="IFunctionActivator"/> implementation generated for the current worker.
    /// </summary>
    internal static class InjectableFunctionsExtensions
    {
        ///<summary>
        /// Configures an optimized function executor to the invocation pipeline.
        ///</summary>
        public static IHostBuilder ConfigureGeneratedFunctionExecutor(this IHostBuilder builder)
        {
            return builder.ConfigureServices(s => 
            {
                s
                .AddTransient<global::MyCompany.MyHttpTriggers>();

                s.AddSingleton<global::Microsoft.Azure.Functions.Worker.IFunctionActivator, global::TestProject.GeneratedFunctionActivator>();
            });
        }
    }


    /// <summary>
    /// Auto startup class to register the custom <see cref="IFunctionActivator"/> implementation generated for the current worker.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class InjectableFunctionsActivator : global::Microsoft.Azure.Functions.Worker.IAutoConfigureStartup
    {
        /// <summary>
        /// Configures the <see cref="IHostBuilder"/> to use the custom <see cref="IFunctionActivator"/> implementation generated for the current worker.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHostBuilder"/> instance to use for service registration.</param>
        public void Configure(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureGeneratedFunctionExecutor();
        }
    }
}