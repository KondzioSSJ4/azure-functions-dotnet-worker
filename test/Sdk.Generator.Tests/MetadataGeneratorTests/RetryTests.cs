using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Functions.SdkGeneratorTests.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Sdk.Generators;
using Microsoft.Azure.Functions.Worker.Sdk.Generators.MetadataGenerator;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Microsoft.Azure.Functions.SdkGeneratorTests.MetadataGeneratorTests
{
    public class RetryTests
    {
        public static IEnumerable<object[]> WithFixedDelay_TestCases()
        {
            static object[] Test(
                int tries,
                string interval,
                Action<RetryModel.RetryInfo?> assert)
                => new object[] { tries, interval, assert };

            Action<RetryModel.RetryInfo?> WithSuccess(TimeSpan expectedDelay)
            {
                return x =>
                {
                    x.Strategy.Should().Be(RetryModel.RetryStrategy.FixedDelay);
                    x.DelayInterval.Should().Be(expectedDelay);
                };
            }

            Action<RetryModel.RetryInfo?> InvalidDelay = x => x.Should().BeNull();

            yield return Test(1, "00:00:10", WithSuccess(TimeSpan.FromSeconds(10)));
            yield return Test(1, "1.00:00:00", WithSuccess(TimeSpan.FromDays(1)));
            yield return Test(-1, "00:00:10", InvalidDelay);
            yield return Test(1, "-00:00:10", InvalidDelay);
            yield return Test(1, "invalidInterval", InvalidDelay);
        }

        [Theory]
        [MemberData(nameof(WithFixedDelay_TestCases))]
        internal Task WithFixedDelay(
            int tries,
            string interval,
            Action<RetryModel.RetryInfo?> assert)
        {
            return Test($$"""
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
                        [FixedDelayRetry({{tries}}, "{{interval}}")]
                        public static void Run([TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo,
                            FunctionContext context)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """,
                assert,
                parametersText: $"{tries}_{interval}",
                additionalAssemblies: new[]
                {
                    typeof(TimerTriggerAttribute).Assembly
                });
        }

        public static IEnumerable<object[]> WithExponentialBackoff_TestCases()
        {
            static object[] Test(
                int tries,
                string minimumInterval,
                string maximumInterval,
                Action<RetryModel.RetryInfo?> assert)
                => new object[] { tries, minimumInterval, maximumInterval, assert };

            Action<RetryModel.RetryInfo?> WithSuccess(
                TimeSpan minimumInterval,
                TimeSpan maximumInterval)
            {
                return x =>
                {
                    x.Strategy.Should().Be(RetryModel.RetryStrategy.ExponentialBackoff);
                    x.MinimumInterval.Should().Be(minimumInterval);
                    x.MaximumInterval.Should().Be(maximumInterval);
                };
            }

            Action<RetryModel.RetryInfo?> InvalidDelay = x => x.Should().BeNull();

            yield return Test(1, "00:00:10", "00:20:00", WithSuccess(TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(20)));
            yield return Test(1, "00:40:10", "00:20:00", InvalidDelay);
            yield return Test(-1, "00:00:10", "00:20:00", InvalidDelay);
            yield return Test(1, "00:00:10", "-00:20:00", InvalidDelay);
            yield return Test(1, "-00:00:10", "00:20:00", InvalidDelay);
        }

        [Theory]
        [MemberData(nameof(WithExponentialBackoff_TestCases))]
        public Task WithExponentialBackoff(
            int tries,
            string minimumInterval,
            string maximumInterval,
            Action<RetryModel.RetryInfo?> assert)
        {
            return Test($$"""
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
                        [ExponentialBackoffRetry({{tries}}, "{{minimumInterval}}", "{{maximumInterval}}")]
                        public static void Run([TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo,
                            FunctionContext context)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """,
                assert,
                $"{tries}_{minimumInterval}_{maximumInterval}",
                additionalAssemblies: new[]
                {
                    typeof(TimerTriggerAttribute).Assembly
                });
        }

        [Fact]
        public Task BothRetry_ShouldReturnDiagnosticError()
        {
            return Test($$"""
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
                        [ExponentialBackoffRetry(1, "00:00:15", "00:30:00")]
                        [FixedDelayRetry(1, "00:15:00")]
                        public static void Run([TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo,
                            FunctionContext context)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """,
                x => x.Should().BeNull(),
                additionalAssemblies: new[]
                {
                    typeof(TimerTriggerAttribute).Assembly
                });
        }

        [Fact]
        public Task WithTriggerNotSupportedRetries()
        {
            return Test($$"""
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                using Microsoft.Extensions.Logging;
                using System.Collections;
                using System.Collections.Generic;
                using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

                namespace TestNamespace
                {
                    public class TestClass
                    {
                        [Function(nameof(Run))]
                        [ExponentialBackoffRetry(5, "00:00:10", "00:15:00")]
                        public static void Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """,
                x => x.Should().BeNull(),
                additionalAssemblies: new[]
                {
                    typeof(HttpTriggerAttribute).Assembly,
                    typeof(HttpRequestData).Assembly
                });
        }

        private async Task Test(
            string sourceCode,
            Action<RetryModel.RetryInfo?> assert,
            string? parametersText = null,
            IReadOnlyCollection<Assembly>? additionalAssemblies = null,
            [CallerMemberName] string callerName = "")
        {
            var emiter = new RetryLoaderEmiter();

            await new SourceGeneratorValidator()
                .WithGenerator(new PrecompiledFunctionMetadataProviderGenerator(new[] { emiter }))
                .WithAssembly(
                    typeof(FunctionAttribute).Assembly,
                    typeof(Task).Assembly)
                .WithAssembly(additionalAssemblies)
                .WithInput(sourceCode)
                .Build()
                .AssertDiagnosticsOfGeneratedCode()
                .VerifyDiagnosticsOnly(
                    parameters: parametersText,
                    callerName: callerName);

            assert.Invoke(emiter.Info);
        }

        private class RetryLoaderEmiter : IPrecompiledFunctionMetadataEmiter
        {
            public RetryModel.RetryInfo? Info { get; private set; }

            public void Emit(
                SourceProductionContext ctx,
                IReadOnlyCollection<FunctionDeclaration> src,
                AnalyzerConfigurationProvider analyzer)
            {
                Info = src.SingleOrDefault()?.Retry?.Info;
            }
        }
    }
}
