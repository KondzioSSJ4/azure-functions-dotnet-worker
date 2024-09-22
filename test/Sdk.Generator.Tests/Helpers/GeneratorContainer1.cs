using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.Azure.Functions.SdkGeneratorTests.Helpers
{
    internal class GeneratorContainer
    {
        private readonly IReadOnlyCollection<IIncrementalGenerator> _incrementalGenerators;
        private readonly IReadOnlyCollection<ISourceGenerator> _sourceGenerators;

        public GeneratorContainer(
            IReadOnlyCollection<IIncrementalGenerator> incrementalGenerators = null,
            IReadOnlyCollection<ISourceGenerator> sourceGenerators = null)
        {
            _incrementalGenerators = incrementalGenerators ?? Array.Empty<IIncrementalGenerator>();
            _sourceGenerators = sourceGenerators ?? Array.Empty<ISourceGenerator>();
        }

        internal GeneratorDriver ToDriver()
        {
            var generators = _sourceGenerators
                .Concat(_incrementalGenerators.Select(GeneratorExtensions.AsSourceGenerator))
                .ToArray();

            if (generators.Length == 0)
            {
                throw new ArgumentException("No generators provided");
            }

            return CSharpGeneratorDriver.Create(generators);
        }
    }
}
