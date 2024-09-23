using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.SdkGeneratorTests.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Xunit;

namespace Microsoft.Azure.Functions.SdkGeneratorTests.PrecompiledFunctionMetadataProviderGeneratorTests
{
    public class RetryTests
    {
        [Theory]
        [InlineData(1, "00:00:10", "valid retry")]
        //[InlineData(1, "1.00:00:10", "valid retry")]
        //[InlineData(-1, "00:00:10", "error")]
        //[InlineData(1, "-00:00:10", "error")]
        //[InlineData(1, "invalidInterval", "error")]
        public Task WithFixedDelay(
            int tries,
            string interval,
            string expectation)
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
                $"{tries}_{interval}_{expectation}",
                additionalAssemblies: new[]
                {
                    typeof(TimerTriggerAttribute).Assembly
                });
        }

        [Theory]
        [InlineData(1, "00:00:10", "00:20:00", "valid retry")]
        [InlineData(1, "00:40:10", "00:20:00", "error")]
        [InlineData(-1, "00:00:10", "00:20:00", "error")]
        [InlineData(1, "00:00:10", "-00:20:00", "error")]
        [InlineData(1, "-00:00:10", "00:20:00", "error")]
        public Task WithExponentialBackoff(
            int tries,
            string minInterval,
            string maxInterval,
            string expectation)
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
                        [ExponentialBackoffRetry({{tries}}, "{{minInterval}}", "{{maxInterval}}")]
                        public static void Run([TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo,
                            FunctionContext context)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """,
                $"{tries}_{minInterval}_{maxInterval}_{expectation}",
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
                additionalAssemblies: new[]
                {
                    typeof(HttpTriggerAttribute).Assembly,
                    typeof(HttpRequestData).Assembly
                });
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
                        typeof(Task).Assembly
                    }.Union(additionalAssemblies ?? Array.Empty<Assembly>()),
                    paramsNames: parameterNames,
                    callerName: callerName);
        }
    }
}
