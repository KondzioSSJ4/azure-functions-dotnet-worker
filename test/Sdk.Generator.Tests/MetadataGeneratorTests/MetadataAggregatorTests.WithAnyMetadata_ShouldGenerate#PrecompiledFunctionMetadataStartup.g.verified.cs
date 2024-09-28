//HintName: PrecompiledFunctionMetadataStartup.g.cs
using System.Linq;
using System.Collections.Immutable;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AzureFunctionInternals.TestProject
{
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal sealed class PrecompiledFunctionMetadataAggregator : global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider
    {
        private readonly global::System.Collections.Generic.IEnumerable<
                global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IAssemblyTypeProvider<
                    global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider>> _providers;

        public PrecompiledFunctionMetadataAggregator(
            global::System.Collections.Generic.IEnumerable<
                global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IAssemblyTypeProvider<
                    global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider>> providers)
        {
            _providers = providers;
        }

        public async global::System.Threading.Tasks.Task<
            global::System.Collections.Immutable.ImmutableArray<
                global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>> GetFunctionMetadataAsync(
            string directory)
        {
            var copy = _providers.Where(x => x.Value is not null).Select(x => x.Value).ToArray();
            if (copy.Length == 1)
            {
                return await copy[0].GetFunctionMetadataAsync(directory);
            }

            var results = await global::System.Threading.Tasks.WhenAll(
                copy.Select(x => x.GetFunctionMetadataAsync(directory)));

            return results.ToImmutableArray();
        }
    }

    /// <summary>
    /// Auto startup class to register the custom <see cref="global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider"/> implementation generated for the current worker.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class PrecompiledFunctionMetadataProviderAutoStartup : global::Microsoft.Azure.Functions.Worker.IAutoConfigureStartup
    {
        /// <summary>
        /// Configures the <see cref="global::Microsoft.Extensions.Hosting.IHostBuilder"/> to use the custom <see cref="IFunctionMetadataProvider"/> implementation generated for the current worker.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="global::Microsoft.Extensions.Hosting.IHostBuilder"/> instance to use for service registration.</param>
        public void Configure(
            global::Microsoft.Extensions.Hosting.IHostBuilder builder)
        {
            builder.ConfigureServices(s =>
            {
                s.AddSingleton<global::AzureFunctionInternals.TestProject.PrecompiledFunctionMetadataProvider>();

                s.AddSingleton<
                    global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IAssemblyTypeProvider<
                        global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider>,
                    global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.AssemblyTypeProvider<
                        global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider,
                        global::AzureFunctionInternals.TestProject.PrecompiledFunctionMetadataProvider>>();

                s.AddSingleton<
                    global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider,
                    global::AzureFunctionInternals.TestProject.PrecompiledFunctionMetadataAggregator>();
            });
        }
    }
}