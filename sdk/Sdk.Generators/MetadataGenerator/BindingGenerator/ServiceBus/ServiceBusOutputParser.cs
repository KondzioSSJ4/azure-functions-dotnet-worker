//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading;
//using Microsoft.Azure.Functions.Worker.Sdk.Generators.Extensions;
//using Microsoft.CodeAnalysis;

//namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator.BindingGenerator.ServiceBus
//{

//    internal enum ServiceBusType
//    {
//        Queue,
//        Topic
//    }

//    public class CollectionWrapper : IEnumerable<object>
//    {
//        public IEnumerator<object> GetEnumerator() => throw new System.NotImplementedException();
//        IEnumerator IEnumerable.GetEnumerator() => throw new System.NotImplementedException();
//    }

//    internal sealed class ServiceBusOutputParser : IGenerateableBindingGenerator
//    {
//        private readonly ISymbol _symbol;
//        private readonly AttributeData _attribute;
//        private readonly string _referenceName;
//        private readonly List<Diagnostic> _diagnostics = new();

//        public ServiceBusOutputParser(
//            ISymbol symbol,
//            AttributeData attribute,
//            string referenceName)
//        {
//            _symbol = symbol;
//            _attribute = attribute;
//            _referenceName = referenceName;
//        }

//        public IReadOnlyCollection<Diagnostic> ParsedDiagnostics => _diagnostics;

//        public IEnumerable<IGenerateableBinding> Generate(CancellationToken cancellationToken)
//        {
//            var queueOrTopicName = _attribute.GetArgumentByConstructor(0)?.Value.ToString() ?? string.Empty;
//            var entityTypeInt = (int)((_attribute.GetArgumentByConstructor(1) ?? _attribute.GetArgumentByName("EntityType"))?.Value ?? 0);
//            var connection = _attribute.GetArgumentByName("Connection")?.Value.ToString();

//            ServiceBusType? entityType = entityTypeInt switch
//            {
//                0 => ServiceBusType.Queue,
//                1 => ServiceBusType.Topic,
//                _ => null
//            };

//            if (!entityType.HasValue)
//            {
//                _diagnostics.Add(Diagnostic.Create(
//                    DiagnosticDescriptors.InvalidBindingType,
//                    _symbol.Locations.FirstOrDefault(),
//                    new[] { entityTypeInt.ToString(), "EntityType" }));
//                yield break;
//            }

//            yield return new ServiceBusOutputBinding(
//                _referenceName,
//                queueOrTopicName,
//                entityType.Value,
//                connection,
//                _attribute.IsRetrySupported());
//        }

//        [DebuggerDisplay("{ToDebugInfo()}")]
//        internal class ServiceBusOutputBinding : IGenerateableBinding
//        {
//            private readonly string _referenceName;
//            private string _queueOrTopicName;
//            private ServiceBusType _type;
//            private string? _connection;

//            public ServiceBusOutputBinding(
//                string referenceName,
//                string queueOrTopicName,
//                ServiceBusType type,
//                string? connection,
//                bool isRetrySupported)
//            {
//                _referenceName = referenceName;
//                _queueOrTopicName = queueOrTopicName;
//                _type = type;
//                _connection = connection;
//                IsRetrySupported = isRetrySupported;
//            }

//            public string BindingName => "serviceBus";

//            public BindingType BindingType => BindingType.Output;

//            public bool IsParsable => true;

//            public bool IsRetrySupported { get; }

//            public string ToGeneratedBinding()
//            {
//                // TODO
//                return ToDebugInfo();
//            }

//            public string ToRawBinding()
//            {
//                // TODO
//                return ToDebugInfo();
//            }

//            private string ToDebugInfo()
//            {
//                return $"ServiceBus output {_referenceName}, {_queueOrTopicName} as {_type} with {_connection}";
//            }
//        }
//    }
//}
