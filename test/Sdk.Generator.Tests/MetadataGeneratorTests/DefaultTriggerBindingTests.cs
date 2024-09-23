using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.SdkGeneratorTests.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;
using Microsoft.Azure.Functions.Worker.Sdk.Generators;
using Microsoft.Azure.Functions.Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator;
using Microsoft.Azure.Functions.Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator.BindingGenerator;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Microsoft.Azure.Functions.SdkGeneratorTests.PrecompiledFunctionMetadataProviderGeneratorTests
{
    public class DefaultTriggerBindingTests
    {
        private class TriggerDeclaredTypeEmiter : IPrecompiledFunctionMetadataEmiter
        {
            private readonly BindingType _expectedType;

            public DataType? Type { get; private set; }

            public TriggerDeclaredTypeEmiter(
                BindingType expectedType)
            {
                _expectedType = expectedType;
            }

            public void Emit(
                SourceProductionContext ctx,
                IReadOnlyCollection<FunctionDeclaration> src,
                AnalyzerConfigurationProvider analyzer)
            {
                var bindingType = src
                    .SelectMany(x => x.Bindings)
                    .Where(x => x.BindingType == _expectedType)
                    .Select(x => new { x.RawType })
                    .FirstOrDefault()?.RawType;

                Type = bindingType?.GetGeneratorDataType();
            }
        }

        [Theory]
        [InlineData("string", false, DataType.String)]
        [InlineData("string[]", false, DataType.String)]
        [InlineData("byte[]", false, DataType.Binary)]
        [InlineData("byte[][]", false, DataType.Binary)]
        [InlineData("ReadOnlyMemory<byte>", false, DataType.Binary)]
        [InlineData("CustomDto", false, DataType.Undefined)]
        [InlineData("IEnumerable<string>", false, DataType.Undefined)]
        [InlineData("IEnumerable<byte[]>", false, DataType.Undefined)]
        [InlineData("IEnumerable<CustomDto>", false, DataType.Undefined)]
        [InlineData("IAsyncEnumerable<string>", false, DataType.Undefined)]
        [InlineData("IAsyncEnumerable<byte[]>", false, DataType.Undefined)]
        [InlineData("IAsyncEnumerable<CustomDto>", false, DataType.Undefined)]
        [InlineData("Task<string>", false, DataType.String)]
        [InlineData("Task<string[]>", false, DataType.String)]
        [InlineData("Task<byte[]>", false, DataType.Binary)]
        [InlineData("Task<CustomDto>", false, DataType.Undefined)]
        [InlineData("ValueTask<string>", false, DataType.String)]
        [InlineData("ValueTask<byte[]>", false, DataType.Binary)]
        [InlineData("ValueTask<CustomDto>", false, DataType.Undefined)]
        [InlineData("string", true, null)]
        [InlineData("byte[]", true, null)]
        [InlineData("CustomDto", true, null)]
        [InlineData("IEnumerable<string>", true, DataType.String)]
        [InlineData("IEnumerable<byte[]>", true, DataType.Binary)]
        [InlineData("IEnumerable<CustomDto>", true, DataType.Undefined)]
        [InlineData("IAsyncEnumerable<string>", true, DataType.String)]
        [InlineData("IAsyncEnumerable<byte[]>", true, DataType.Binary)]
        [InlineData("IAsyncEnumerable<CustomDto>", true, DataType.Undefined)]
        [InlineData("Task<string>", true, null)]
        [InlineData("Task<byte[]>", true, null)]
        [InlineData("Task<CustomDto>", true, null)]
        [InlineData("ValueTask<string>", true, null)]
        [InlineData("ValueTask<byte[]>", true, null)]
        [InlineData("ValueTask<CustomDto>", true, null)]
        public async Task WithDifferentInputTypes(
            string inputType,
            bool isBatched,
            DataType? expectedType)
        {
            var emiter = new TriggerDeclaredTypeEmiter(BindingType.Trigger);
            var generator = new PrecompiledFunctionMetadataProviderGenerator(new[] { emiter });

            await new SourceGeneratorValidator()
                .WithGenerator(generator)
                .WithAssemblies(typeof(ServiceBusTriggerAttribute).Assembly)
                .WithInput($$"""
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Extensions.Logging;
                using System.Collections.Generic;
                using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

                namespace TestNamespace
                {
                    public class TestClass
                    {
                        [Function(nameof(Run))]
                        public Task Run(
                            [ServiceBusTrigger(
                                "queueName", 
                                IsBatched = {{isBatched.ToString().ToLower()}})]
                            {{inputType}} value)
                        {
                            throw new NotImplementedException();
                        }

                        public class CustomDto { }
                    }
                }
                """)
                .Build()
                .ValidateGeneratorDiagnostics(d =>
                {
                    var errors = d.Where(x => x.Severity >= DiagnosticSeverity.Error);
                    if (expectedType.HasValue)
                    {
                        Assert.Empty(errors);
                    }
                    else
                    {
                        Assert.NotEmpty(errors);
                    }
                });

            Assert.Equal(expectedType, emiter.Type);
        }

        [Theory]
        [InlineData("string", @"""SomeStringValue""")]
        [InlineData("string[]", @"new [] { ""SomeStringValue"" }")]
        [InlineData("int", "2")]
        [InlineData("Type", "typeof(TestClass)")]
        [InlineData("SomeEnum", "SomeEnum.First")]
        public Task WithTriggerCtorAttribute(
            string inputType,
            string inputValue)
        {
            return Test($$"""
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Extensions.Logging;
                using System.Collections.Generic;
                using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

                namespace TestNamespace
                {
                    public class TestClass
                    {
                        [Function(nameof(Run))]
                        public Task Run(
                            [CustomBindingTrigger({{inputValue}})] object value)
                        {
                            throw new NotImplementedException();
                        }
                    }

                    public class CustomBindingTrigger : TriggerBindingAttribute
                    {
                        public CustomBindingTrigger({{inputType}} input)
                        {
                        }
                    }

                    public enum SomeEnum
                    {
                        Unknown,
                        First
                    }
                }
                """,
                parameterNames: inputType);
        }

        [Theory]
        [InlineData(true, "object", "error")]
        [InlineData(true, "string", "error")]
        [InlineData(true, "Task<string>", "error")]
        [InlineData(false, "string", "binding")]
        [InlineData(true, "IEnumerable<object>", "binding")]
        [InlineData(true, "CollectionWrapper", "error")]
        [InlineData(true, "List<object>", "binding")]
        [InlineData(true, "object[]", "binding")]
        [InlineData(true, "IReadOnlyCollection<object>", "binding")]
        [InlineData(true, "IAsyncEnumerable<object>", "binding")]
        public Task WithBatchTrigger(
            bool isBatched,
            string inputType,
            string expectations)
        {
            return Test(
                $$"""
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Extensions.Logging;
                using System.Collections;
                using System.Collections.Generic;
                using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;
                
                namespace TestNamespace
                {
                    public class TestClass
                    {
                        [Function(nameof(Run))]
                        public Task Run(
                            [ServiceBusTrigger("queueName", IsBatched = {{isBatched.ToString().ToLower()}})] {{inputType}} value)
                        {
                            throw new NotImplementedException();
                        }
                    }

                    public class CollectionWrapper : IEnumerable<object>
                    {
                        public IEnumerator<object> GetEnumerator() => throw new System.NotImplementedException();
                        IEnumerator IEnumerable.GetEnumerator() => throw new System.NotImplementedException();
                    }
                }
                """,
                parameterNames: $"{isBatched}_{inputType}_{expectations}",
                additionalAssemblies: new[]
                {
                    typeof(ServiceBusTriggerAttribute).Assembly
                });
        }

        [Theory]
        [InlineData("One")]
        [InlineData("Many")]
        public Task WithCardinalityByProperty(string cardinality)
        {
            return Test(
                $$"""
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Extensions.Logging;
                using System.Collections;
                using System.Collections.Generic;
                using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;
                
                namespace TestNamespace
                {
                    public class TestClass
                    {
                        [Function(nameof(Run))]
                        public Task Run(
                            [CustomTrigger(Cardinality = Cardinality.{{cardinality}})] IEnumerable<object> value)
                        {
                            throw new NotImplementedException();
                        }
                    }
                    
                    public class CustomTrigger : TriggerBindingAttribute, ISupportCardinality
                    {
                        public Cardinality Cardinality { get; set; }
                    }
                }
                """,
                parameterNames: cardinality);
        }

        [Fact]
        public Task WithNamedArguments()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task WithReplacedValueInProperty()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task WithAoT_ShouldThrowDiagnostic()
        {
            throw new NotImplementedException();
        }

        private async Task Test(
            string sourceCode,
            string parameterNames = null,
            IReadOnlyCollection<Assembly> additionalAssemblies = null,
            [CallerMemberName] string callerName = "")
        {
            await new Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator.PrecompiledFunctionMetadataProviderGenerator()
            //await new Worker.Sdk.Generators.FunctionMetadataProviderGenerator()
                .RunAndVerify(
                    sourceCode,
                    new[]
                    {
                        typeof(FunctionAttribute).Assembly,
                        typeof(Task).Assembly,
                        typeof(TriggerBindingAttribute).Assembly
                    }.Union(additionalAssemblies ?? Array.Empty<Assembly>()),
                    paramsNames: parameterNames,
                    callerName: callerName);
        }
    }
}
