using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.MetadataGenerator.BindingGenerator
{
    public enum BindingType
    {
        Unknown,
        Trigger,
        Input,
        Output
    }

    public interface IGenerateableBinding
    {
        string BindingName { get; }

        BindingType BindingType { get; }

        DataType DeclaredType { get; }

        bool IsParsable { get; }

        bool IsRetrySupported { get; }

        string ToRawBinding();

        string ToGeneratedBinding();
    }

    public interface IGenerateableBindingGenerator
    {
        IEnumerable<IGenerateableBinding> Generate(CancellationToken cancellationToken);

        IReadOnlyCollection<Diagnostic> ParsedDiagnostics { get; }
    }
}
