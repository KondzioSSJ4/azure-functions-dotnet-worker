using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.SdkGeneratorTests.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Sdk.Generators.MetadataGenerator;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Microsoft.Azure.Functions.SdkGeneratorTests.MetadataGeneratorTests
{
    public class ServiceBusTests
    {
        [Fact]
        public async Task WithAliasOutputAttribute()
        {
            await Test("""
                using System;
                using System.Threading.Tasks;
                using Azure.Messaging.ServiceBus;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Extensions.Logging;
                using Meh = Microsoft.Azure.Functions.Worker.ServiceBusOutputAttribute;

                namespace SampleApp
                {
                    public class FunctionClass 
                    {
                        [Function(nameof(MethodName))]
                        [Meh("outputQueue1", Connection = "ServiceBusConnection")]
                        public string MethodName(
                            [ServiceBusTrigger("queue1", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """);
        }

        [Fact]
        public async Task WithoutOutputAttribute_ShouldThrowDiagnostic()
        {
            await Test("""
                using System;
                using System.Threading.Tasks;
                using Azure.Messaging.ServiceBus;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Extensions.Logging;

                namespace SampleApp
                {
                    public class FunctionClass 
                    {
                        [Function(nameof(MethodName))]
                        public string MethodName(
                            [ServiceBusTrigger("queue1", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """);
        }

        [Fact]
        public async Task WithAsyncVoid_ShoudThrowDiagnostic()
        {
            await Test("""
                using System;
                using System.Threading.Tasks;
                using Azure.Messaging.ServiceBus;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Extensions.Logging;

                namespace SampleApp
                {
                    public class FunctionClass 
                    {
                        [Function(nameof(MethodName))]
                        public async void MethodName(
                            [ServiceBusTrigger("queue1", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """);
        }

        [Fact]
        public async Task WithOutputNamedCtorValues()
        {
            await Test(
                """
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Azure.Messaging.ServiceBus;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Extensions.Logging;
                using System.Collections.Generic;
                using TaskOfInts = System.Threading.Tasks.Task<int>;
                
                namespace SampleApp
                {
                    public class TestClass
                    {
                        [Function(nameof(SwapCtorArguments))]
                        [ServiceBusOutput(entityType: ServiceBusEntityType.Topic, queueOrTopicName: "queue1", Connection = "connection")]
                        public Task<string> SwapCtorArguments(
                            [ServiceBusTrigger("queue1", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """);
        }

        [Fact]
        public async Task WithOutputDefaultArguments()
        {
            await Test(
                """
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Azure.Messaging.ServiceBus;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Extensions.Logging;
                using System.Collections.Generic;
                using TaskOfInts = System.Threading.Tasks.Task<int>;
                
                namespace SampleApp
                {
                    public class TestClass
                    {
                        [Function(nameof(SwapCtorArguments))]
                        [ServiceBusOutput("queue1")]
                        public Task<string> SwapCtorArguments(
                            [ServiceBusTrigger("queue1", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """);
        }

        public enum BindingType
        {
            None,
            ServiceBus,
        }

        [Theory]
        //[InlineData("void")]
        //[InlineData("Task")]
        //[InlineData("string")]
        //[InlineData("Task<string>")]
        //[InlineData("ValueTask<string>")]
        //[InlineData("int")]
        //[InlineData("int?")]
        //[InlineData("Task<int>")]
        //[InlineData("TaskOfInts")]
        //[InlineData("Task<List<int>>")]
        //[InlineData("Task<int[]>")]
        //[InlineData("int[]")]
        //[InlineData("List<int>")]
        //[InlineData("ValueTask<int>")]
        //[InlineData("OutputType")]
        //[InlineData("Task<OutputType>", "2x bindings")]
        //[InlineData("ValueTask<OutputType>")]
        //[InlineData("IAsyncEnumerable<int>")]
        //[InlineData("AsyncCollection<int>")]
        //[InlineData("AsyncCollection<SomeDto>")]
        //[InlineData("SomeDto")]
        [InlineData("Task<SomeDto>", BindingType.ServiceBus, "1x binding")]
        //[InlineData("Task<SomeDto>", false)]
        public async Task WithOutputResults(
            string returnType,
            BindingType methodBinding,
            string expectations)
        {
            var attribute = methodBinding switch
            {
                BindingType.None => string.Empty,
                BindingType.ServiceBus => @"[ServiceBusOutput(""outputQueue1"", Connection = ""ServiceBusConnection"")]",
                _ => throw new NotImplementedException()
            };

            await Test($$"""
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Azure.Messaging.ServiceBus;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Extensions.Logging;
                using System.Collections.Generic;
                using TaskOfInts = System.Threading.Tasks.Task<int>;
                
                namespace SampleApp
                {
                    public class TestClass
                    {
                        [Function(nameof(Run))]
                        {{attribute}}
                        public {{returnType}} Run(
                            [ServiceBusTrigger("queue1", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message)
                        {
                            throw new NotImplementedException();
                        }

                        public class OutputType
                        {
                            [ServiceBusOutput("TopicOrQueueName1", Connection = "ServiceBusConnection")]
                            public string OutputEvent1 { get; set; }

                            [ServiceBusOutput("TopicOrQueueName2", Connection = "ServiceBusConnection")]
                            public string OutputEvent2 { get; set; }
                        }

                        public class SomeDto
                        {
                            public string Value { get;set; }
                        }

                        public class AsyncCollection<T> : IAsyncEnumerable<T>
                        {
                            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                            {
                                throw new NotImplementedException();
                            }
                        }
                    }
                }
                """,
                parameterNames: $"{returnType}_{expectations}");
        }

        private async Task Test(
            string sourceCode,
            LanguageVersion languageVersion = LanguageVersion.CSharp10,
            string parameterNames = null,
            [CallerMemberName] string callerName = "")
        {
            await new SourceGeneratorValidator() { LanguageVersion = languageVersion }
                .WithGenerator(new PrecompiledFunctionMetadataProviderGenerator())
                .WithAssembly(
                    typeof(FunctionAttribute).Assembly,
                    typeof(Task).Assembly,
                    typeof(ServiceBusTriggerAttribute).Assembly)
                .WithInput(sourceCode)
                .Build()
                .AssertDiagnosticsOfGeneratedCode()
                .VerifySpecifiedFile(
                    BindingDeclarationEmiter.AssemblyMetadataFile,
                    parameters: parameterNames,
                    callerName: callerName);
        }
    }
}
