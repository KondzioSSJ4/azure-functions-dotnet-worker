using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator.BindingGenerator
{
    internal enum BindingType
    {
        Unknown,
        Trigger,
        Input,
        Output
    }

    internal interface IGenerateableBinding
    {
        string BindingName { get; }

        BindingType Type { get; }

        bool IsParsable { get; }

        bool IsRetrySupported { get; }

        string ToRawBinding();

        string ToGeneratedBinding();
    }

    internal interface IGenerateableBindingGenerator
    {
        IEnumerable<IGenerateableBinding> Generate(CancellationToken cancellationToken);

        IReadOnlyCollection<Diagnostic> ParsedDiagnostics { get; }
    }
}
