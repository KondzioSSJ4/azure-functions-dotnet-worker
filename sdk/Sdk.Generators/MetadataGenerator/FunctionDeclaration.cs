using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Functions.Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator.BindingGenerator;
using Microsoft.CodeAnalysis;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator
{
    public sealed record FunctionDeclaration
    {
        public string FunctionName { get; internal set; }
        public string MethodName { get; set; }
        public string ContainingTypeName { get; set; }
        public List<IGenerateableBinding> Bindings { get; } = new();
        public RetryModel? Retry { get; set; }
        public IEnumerable<string> BindingsText => Bindings.Select(x => x.ToString());

    }
}
