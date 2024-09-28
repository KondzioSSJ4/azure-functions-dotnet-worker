using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.SdkGeneratorTests.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Sdk.Generators.MetadataGenerator;
using Xunit;

namespace Microsoft.Azure.Functions.SdkGeneratorTests.MetadataGeneratorTests
{
    public class MetadataAggregatorTests
    {
        [Fact]
        public Task WithAnyMetadata_ShouldGenerate()
        {
            return Test(
                """
                using System;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;

                namespace AspNetIntegration
                {
                    public class SimpleHttpTriggerHttpData
                    {
                        [Function("SimpleHttpTriggerHttpData")]
                        public async Task<HttpResponseData> Run(
                            [HttpTrigger(AuthorizationLevel.Anonymous)]
                            HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """);
        }

        [Fact]
        public Task WithoutAnyMetadata_ShouldNotGenerate()
        {
            return Test(
                """
                using System;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;

                namespace AspNetIntegration
                {
                    public class SimpleHttpTriggerHttpData
                    {
                        public async Task<HttpResponseData> Run(object req)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """);
        }

        private async Task Test(
            string sourceCode,
            [CallerMemberName] string callerName = "")
        {
            await new SourceGeneratorValidator()
                .WithGenerator(new PrecompiledFunctionMetadataProviderGenerator())
                .WithAssembly(
                    typeof(HttpTriggerAttribute).Assembly,
                    typeof(FunctionAttribute).Assembly,
                    typeof(HttpRequest).Assembly)
                .WithInput(sourceCode)
                .Build()
                .AssertDiagnosticsOfGeneratedCode()
                .VerifySpecifiedFile(
                    BindingDeclarationEmiter.AggregatorFile,
                    callerName: callerName);
        }
    }
}
