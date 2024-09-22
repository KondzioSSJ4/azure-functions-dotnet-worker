using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.SdkGeneratorTests.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;
using Xunit;

namespace Microsoft.Azure.Functions.SdkGeneratorTests.PrecompiledFunctionMetadataProviderGeneratorTests
{
    public class DefaultTriggerBindingTests
    {
        [Theory]
        [InlineData("string", false, "String")]
        [InlineData("byte[]", false, "Binary")]
        [InlineData("CustomDto", false, "Unknown")]
        [InlineData("IEnumerable<string>", false, "Unknown")]
        [InlineData("IEnumerable<byte[]>", false, "Unknown")]
        [InlineData("IEnumerable<CustomDto>", false, "Unknown")]
        [InlineData("IAsyncEnumerable<string>", false, "Unknown")]
        [InlineData("IAsyncEnumerable<byte[]>", false, "Unknown")]
        [InlineData("IAsyncEnumerable<CustomDto>", false, "Unknown")]
        [InlineData("Task<string>", false, "String")]
        [InlineData("Task<byte[]>", false, "Binary")]
        [InlineData("Task<CustomDto>", false, "Unknown")]
        [InlineData("ValueTask<string>", false, "String")]
        [InlineData("ValueTask<byte[]>", false, "Binary")]
        [InlineData("ValueTask<CustomDto>", false, "Unknown")]
        [InlineData("string", true, "error")]
        [InlineData("byte[]", true, "error")]
        [InlineData("CustomDto", true, "error")]
        [InlineData("IEnumerable<string>", true, "String")]
        [InlineData("IEnumerable<byte[]>", true, "Binary")]
        [InlineData("IEnumerable<CustomDto>", true, "Unknown")]
        [InlineData("IAsyncEnumerable<string>", true, "String")]
        [InlineData("IAsyncEnumerable<byte[]>", true, "Binary")]
        [InlineData("IAsyncEnumerable<CustomDto>", true, "Unknown")]
        [InlineData("Task<string>", true, "error")]
        [InlineData("Task<byte[]>", true, "error")]
        [InlineData("Task<CustomDto>", true, "error")]
        [InlineData("ValueTask<string>", true, "error")]
        [InlineData("ValueTask<byte[]>", true, "error")]
        [InlineData("ValueTask<CustomDto>", true, "error")]
        public Task WithDifferentInputTypes(
            string inputType,
            bool isBatched,
            string expectedType)
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
                """,
                parameterNames: inputType,
                additionalAssemblies: new[]
                {
                    typeof(ServiceBusTriggerAttribute).Assembly
                });
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
