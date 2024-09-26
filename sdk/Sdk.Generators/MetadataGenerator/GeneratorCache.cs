using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.MetadataGenerator
{
    internal sealed class GeneratorCache
    {
        private readonly Compilation _compilation;
        private readonly object _lock = new object();

        private readonly Dictionary<string, INamedTypeSymbol?> _namedTypes = new(StringComparer.OrdinalIgnoreCase);

        public GeneratorCache(
            Compilation compilation)
        {
            _compilation = compilation;
        }

        public INamedTypeSymbol? BindingAttribute => GetNamedType(Constants.Types.BindingAttribute);
        public INamedTypeSymbol? HttpTriggerBinding => GetNamedType(Constants.Types.HttpTriggerBinding);
        public INamedTypeSymbol? InputConverterAttributeType => GetNamedType(Constants.Types.InputConverterAttributeType);
        public INamedTypeSymbol? SupportsDeferredBindingAttributeType => GetNamedType(Constants.Types.SupportsDeferredBindingAttributeType);
        public INamedTypeSymbol? SupportedTargetTypeAttributeType => GetNamedType(Constants.Types.SupportedTargetTypeAttributeType);

        public INamedTypeSymbol? GetNamedType(string name)
            => GetFromCache(_namedTypes, name, () => _compilation.GetTypeByMetadataName(name));

        private TValue? GetFromCache<TKey, TValue>(
            Dictionary<TKey, TValue?> map,
            TKey key,
            Func<TValue> loadValue)
        {
            if (map.TryGetValue(key, out var value))
            {
                return value;
            }

            lock (_lock)
            {
                if (map.TryGetValue(key, out value))
                {
                    return value;
                }

                var result = loadValue.Invoke();
                map.Add(key, result);
                return result;
            }
        }
    }
}
