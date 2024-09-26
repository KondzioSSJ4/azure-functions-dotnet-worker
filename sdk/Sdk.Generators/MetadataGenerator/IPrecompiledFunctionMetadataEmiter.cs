using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.MetadataGenerator
{
    public interface IPrecompiledFunctionMetadataEmiter
    {
        void Emit(
            SourceProductionContext ctx,
            IReadOnlyCollection<FunctionDeclaration> src,
            AnalyzerConfigurationProvider analyzer);
    }
}
