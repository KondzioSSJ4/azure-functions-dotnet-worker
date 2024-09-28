using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.SdkGeneratorTests.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Sdk.Generators.MetadataGenerator;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Microsoft.Azure.Functions.SdkGeneratorTests.MetadataGeneratorTests
{
    public class HttpTriggerTests
    {
        [Fact]
        public async Task FunctionWithoutTrigger()
        {
            await Test("""
                using System;
                using System.Collections.Generic;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                
                namespace FunctionApp
                {
                    public static class HttpTriggerSimple
                    {
                        [Function(nameof(Run))]
                        public static Task Run(string input)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """,
                LanguageVersion.CSharp10);
        }

        [Theory]
        [ClassData(typeof(SupportedLanguageTestCases))]
        public async Task GenerateSingleHttpTrigger(LanguageVersion languageVersion)
        {
            await Test("""
                using System;
                using System.Collections.Generic;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;

                namespace FunctionApp
                {
                    public static class HttpTriggerSimple
                    {
                        [Function(nameof(HttpTriggerSimple))]
                        public static Task Run(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """,
                languageVersion);
        }

        [Theory]
        [ClassData(typeof(SupportedLanguageTestCases))]
        public async Task Generate_WithManyDifferentProviders(LanguageVersion languageVersion)
        {
            await Test("""
                using Microsoft.AspNetCore.Http;
                using Microsoft.AspNetCore.Mvc;
                using Microsoft.Azure.Functions.Worker;
                using System.IO;

                namespace AspNetIntegration
                {
                    public class FileDownload
                    {
                        private const string BlobContainer = "runtimes";
                        private const string BlobName = "dotnet-sdk-8.0.100-win-x64.exe";

                        [Function("FileDownload")]
                        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req,
                            [BlobInput($"{BlobContainer}/{BlobName}")] Stream blobStream)
                        {
                            return new FileStreamResult(blobStream, "application/octet-stream")
                            {
                                FileDownloadName = BlobName
                            };
                        }
                    }
                }
                
                """,
                languageVersion);
        }

        [Theory]
        [ClassData(typeof(SupportedLanguageTestCases))]
        public async Task Generate_InsideOfRecord(LanguageVersion languageVersion)
        {
            await Test("""
                using System;
                using System.Collections.Generic;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                
                namespace FunctionApp
                {
                    public record HttpTriggerSimple()
                    {
                        [Function(nameof(HttpTriggerSimple))]
                        public static Task Run(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """,
                languageVersion);
        }

        [Theory]
        [ClassData(typeof(SupportedLanguageTestCases))]
        public async Task Generate_WithManyOutputBindings(LanguageVersion languageVersion)
        {
            await Test("""
                using System;
                using System.Collections.Generic;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                using Microsoft.AspNetCore.Mvc;
                
                namespace FunctionApp
                {
                    public class HttpTriggerSimple
                    {
                        [Function(nameof(WithNestedOutput))]
                        public static NestedOutputTypes WithNestedOutput(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }

                        [Function(nameof(WithNestedOutputTypesWithoutTypeDefinedOutput))]
                        public static NestedOutputTypesWithoutTypeDefinedOutput WithNestedOutputTypesWithoutTypeDefinedOutput(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }

                        [Function(nameof(ByAttribute))]
                        public static NestedOutputWithHttpAttribute ByAttribute(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }

                        [Function(nameof(GenericString))]
                        public static NestedHttpWithGeneric<string> GenericString(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }

                        [Function(nameof(GenericIActionResult))]
                        public static NestedHttpWithGeneric<IActionResult> GenericIActionResult(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }

                        [Function(nameof(GenericHttpResponseData))]
                        public static NestedHttpWithGeneric<HttpResponseData> GenericHttpResponseData(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }
                    }

                    public class NestedOutputTypes
                    {
                        [QueueOutput("functionstesting2", Connection = "AzureWebJobsStorage")]
                        public string Name { get; set; }
                
                        public HttpResponseData HttpResponse { get; set; }
                    }


                    public class NestedOutputTypesWithoutTypeDefinedOutput
                    {
                        [QueueOutput("functionstesting2", Connection = "AzureWebJobsStorage")]
                        public string Name { get; set; }
                
                        public string ItsNotABindingToAnything { get; set; }
                    }

                    public class NestedOutputWithHttpAttribute
                    {
                        [QueueOutput("functionstesting2", Connection = "AzureWebJobsStorage")]
                        public string Name { get; set; }
                
                        [HttpResult]
                        public string HttpResponse { get; set; }
                    }

                    public class NestedHttpWithGeneric<T>
                    {
                        [QueueOutput("functionstesting2", Connection = "AzureWebJobsStorage")]
                        public string Name { get; set; }
                
                        [HttpResult]
                        public T Response { get; set; }
                    }
                }
                """,
                languageVersion,
                new[]
                {
                    typeof(IActionResult).Assembly,
                    typeof(QueueOutputAttribute).Assembly
                });
        }

        [Theory]
        [InlineData("string")]
        [InlineData("IReadOnlyCollection<string>")]
        [InlineData("IEnumerable<string>")]
        [InlineData("string[]")]
        [InlineData("List<string>")]
        [InlineData("NestedOutput")]
        [InlineData("IReadOnlyCollection<NestedOutput>")]
        [InlineData("IEnumerable<NestedOutput>")]
        [InlineData("NestedOutput[]")]
        [InlineData("List<NestedOutput>")]
        public async Task Generate_WithCardinalTypes(
            string typeName)
        {
            await Test($$"""
                using System;
                using System.Collections.Generic;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                
                namespace FunctionApp
                {
                    public class HttpTriggerSimple
                    {
                        [Function(nameof(HttpTriggerSimple))]
                        public static {{typeName}} Run(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }
                    }
                
                    public class NestedOutput
                    {
                        [QueueOutput("functionstesting2", Connection = "AzureWebJobsStorage")]
                        public string Name { get; set; }
                    }
                }

                """,
                LanguageVersion.CSharp10,
                new[]
                {
                    typeof(QueueOutputAttribute).Assembly
                },
                paramsNames: $"Type={typeName}");
        }

        [Theory]
        [ClassData(typeof(SupportedLanguageTestCases))]
        public async Task Generate_WithNestedOutput(LanguageVersion languageVersion)
        {
            await Test("""
                using System;
                using System.Collections.Generic;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                
                namespace FunctionApp
                {
                    public class HttpTriggerSimple
                    {
                        [Function(nameof(HttpTriggerSimple))]
                        public static MyOutputType Run(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }
                    }

                    public class MyOutputType
                    {
                        [QueueOutput("functionstesting2", Connection = "AzureWebJobsStorage")]
                        public string Name { get; set; }
                    }
                }
                """,
                languageVersion,
                new[]
                {
                    typeof(QueueOutputAttribute).Assembly
                });
        }

        [Theory]
        [ClassData(typeof(SupportedLanguageTestCases))]
        public async Task Generate_FunctionsMultipleOutputBindingWithActionResult(LanguageVersion languageVersion)
        {
            await Test("""
                using System;
                using System.Diagnostics.CodeAnalysis;
                using System.Net;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                using Microsoft.AspNetCore.Http;
                using Microsoft.AspNetCore.Mvc;
                
                namespace FunctionApp
                {
                    public static class FunctionsMultipleOutputBindingWithActionResult
                    {
                        [Function(nameof(FunctionsMultipleOutputBindingWithActionResult))]
                        public static MyOutputType Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
                            FunctionContext context)
                        {
                            throw new NotImplementedException();
                        }
                
                        [Function("OutputTypeHttpHasTwoAttributes")]
                        public static MyOutputType2 Test([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
                            FunctionContext context)
                        {
                            throw new NotImplementedException();
                        }
                    }
                
                    public class MyOutputType
                    {
                        [QueueOutput("functionstesting2", Connection = "AzureWebJobsStorage")]
                        public string Name { get; set; }
                
                        [HttpResult]
                        public IActionResult HttpResponse { get; set; }
                    }
                
                    public class MyOutputType2
                    {
                        [QueueOutput("functionstesting2", Connection = "AzureWebJobsStorage")]
                        public string Name { get; set; }
                
                        [SuppressMessage("Microsoft.Naming", "Foo", Justification = "Bar")]
                        [HttpResult]
                        public IActionResult HttpResponse { get; set; }
                    }
                }
                """,
                languageVersion,
                new[]
                {
                    typeof(HttpRequest).Assembly,
                    typeof(QueueOutputAttribute).Assembly,
                    typeof(IActionResult).Assembly
                });
        }

        [Theory]
        [ClassData(typeof(SupportedLanguageTestCases))]
        public async Task Generate_FunctionWithStringDataTypeInputBinding(LanguageVersion languageVersion)
        {
            await Test("""
                using System.Net;
                using System.Text.Json;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                
                namespace FunctionApp
                {
                    public static class HttpTriggerWithBlobInput
                    {
                        [Function(nameof(HttpTriggerWithBlobInput))]
                        public static MyOutputType Run(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req,
                            [BlobInput("test-samples/sample1.txt", Connection = "AzureWebJobsStorage")] string myBlob, FunctionContext context)
                        {
                            var bookVal = (Book)JsonSerializer.Deserialize(myBlob, typeof(Book));
                
                            var response = req.CreateResponse(HttpStatusCode.OK);
                
                            response.Headers.Add("Date", "Mon, 18 Jul 2016 16:06:00 GMT");
                            response.Headers.Add("Content-Type", "text/html; charset=utf-8");
                            response.WriteString("Book Sent to Queue!");
                
                            return new MyOutputType()
                            {
                                Book = bookVal,
                                HttpResponse = response
                            };
                        }
                
                        public class MyOutputType
                        {
                            [QueueOutput("functionstesting2", Connection = "AzureWebJobsStorage")]
                            public Book Book { get; set; }
                
                            public HttpResponseData HttpResponse { get; set; }
                        }
                
                        public class Book
                        {
                            public string name { get; set; }
                            public string id { get; set; }
                        }
                    }
                }
                """,
                languageVersion,
                new[]
                {
                    typeof(BlobInputAttribute).Assembly,
                    typeof(QueueOutputAttribute).Assembly
                });
        }

        [Theory]
        [ClassData(typeof(SupportedLanguageTestCases))]
        public async Task Generate_FunctionWithNonFunctionsRelatedAttribute(LanguageVersion languageVersion)
        {
            await Test("""
                using System;
                using System.Net;
                using System.Text.Json;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                
                namespace FunctionApp
                {
                    public class HttpTriggerWithBlobInput
                    {
                        [Function("Products")]
                        public HttpResponseData Run(
                                       [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req,
                                       [FakeAttribute("hi")] string someString)
                        {
                            var response = req.CreateResponse(HttpStatusCode.OK);
                            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                            return response;
                        }
                    }
                
                    public class FakeAttribute : Attribute
                    {
                        public FakeAttribute(string name)
                        {
                            Name = name;
                        }
                
                        public string Name { get; }
                    }
                }
                """,
                languageVersion);
        }

        [Theory]
        [ClassData(typeof(SupportedLanguageTestCases))]
        public async Task Generate_FunctionWithTaskReturnType(LanguageVersion languageVersion)
        {
            await Test("""
                using System;
                using System.Net;
                using System.Text.Json;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                
                namespace FunctionApp
                {
                    public class Timer
                    {
                        [Function("TimerFunction")]
                        public Task RunTimer([TimerTrigger("0 0 0 * * *", RunOnStartup = false)] object timer)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """,
                languageVersion,
                new[]
                {
                    typeof(TimerTriggerAttribute).Assembly
                });
        }

        [Theory]
        [ClassData(typeof(SupportedLanguageTestCases))]
        public async Task Generate_FunctionWithGenericTaskReturnType(LanguageVersion languageVersion)
        {
            await Test("""
                using System;
                using System.Net;
                using System.Text.Json;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                
                namespace FunctionApp
                {
                    public class BasicHttp
                    {
                        [Function("FunctionName")]
                        public Task<HttpResponseData> Http([HttpTrigger(AuthorizationLevel.Admin, "get", "Post", Route = "/api2")] HttpRequestData myReq)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """,
                languageVersion);
        }

        [Theory]
        [ClassData(typeof(SupportedLanguageTestCases))]
        public async Task Generate_MultipleFunctionsMetadata(LanguageVersion languageVersion)
        {
            await Test("""
                using System;
                using System.Threading.Tasks;
                using System.Collections.Generic;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                
                namespace FunctionApp
                {
                    public class HttpTriggerSimple
                    {
                        [Function(nameof(Run))]
                        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req, FunctionContext executionContext)
                        {
                            throw new NotImplementedException();
                        }
                        [Function(nameof(RunAsync))]
                        public Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }
                        [Function(nameof(RunAsync2))]
                        public async Task<HttpResponseData> RunAsync2([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """,
                languageVersion);
        }

        [Theory]
        [ClassData(typeof(SupportedLanguageTestCases))]
        public async Task Generate_HttpTriggerVoidOrTaskReturnType(LanguageVersion languageVersion)
        {
            await Test("""
                using System;
                using System.Collections.Generic;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.AspNetCore.Http;
                using System.Net.Http;
                using System.Threading;
                using System.Threading.Tasks;
                
                namespace Foo
                {
                    public sealed class HttpTriggers
                    {
                        [Function("Function1")]
                        public Task Foo([HttpTrigger("get")] HttpRequest r) => throw new NotImplementedException();
                
                        [Function("Function2")]
                        public void Bar([HttpTrigger("get")] HttpRequest req)  => throw new NotImplementedException();
                
                        [Obsolete("This method is obsolete. Use Foo instead.")]
                        [Function("Function3")]
                        public Task Baz([HttpTrigger("get")] HttpRequest r) => throw new NotImplementedException();
                    }
                }
                """,
                languageVersion);
        }

        [Theory]
        [ClassData(typeof(SupportedLanguageTestCases))]
        public async Task Generate_WithDifferentTriggerCtors_ShouldParse(LanguageVersion languageVersion)
        {
            await Test("""
                using System;
                using System.Collections.Generic;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.AspNetCore.Http;
                using System.Net.Http;
                using System.Threading;
                using System.Threading.Tasks;
                
                namespace Foo
                {
                    public sealed class HttpTriggersWithPlainArguments
                    {
                        [Function("FunctionExactName")]
                        public Task FunctionExactNameMethod([HttpTrigger] HttpRequest r) => throw new NotImplementedException();

                        [Function(nameof(WithDefaultCtorShort))]
                        public Task WithDefaultCtorShort([HttpTrigger] HttpRequest r) => throw new NotImplementedException();

                        [Function(nameof(WithDefaultCtor))]
                        public Task WithDefaultCtor([HttpTrigger()] HttpRequest r) => throw new NotImplementedException();
                
                        [Function(nameof(WithMethodsOnly))]
                        public Task WithMethodsOnly([HttpTrigger("get", "post")] HttpRequest r) => throw new NotImplementedException();
                
                        [Function(nameof(WithAuthMethodOnly))]
                        public Task WithAuthMethodOnly([HttpTrigger(AuthorizationLevel.Function)] HttpRequest r) => throw new NotImplementedException();
                
                        [Function(nameof(WithAuthLevelAndMethods))]
                        public Task WithAuthLevelAndMethods([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest r) => throw new NotImplementedException();
                
                        [Function(nameof(WithAuthLevelAndMethodsAndRoutes))]
                        public Task WithAuthLevelAndMethodsAndRoutes([HttpTrigger(AuthorizationLevel.Function, "get", Route = "route")] HttpRequest r) => throw new NotImplementedException();
                    }

                    public sealed class HttpTriggersWithConsts
                    {
                        private const AuthorizationLevel DefaultLevel = AuthorizationLevel.Function;
                        private const string Method = "post";
                        private const string Prefix = "function-prefix";

                        [Function("FunctionExactName")]
                        public Task FunctionExactNameMethod([HttpTrigger] HttpRequest r) => throw new NotImplementedException();
                
                        [Function(nameof(WithDefaultCtorShort))]
                        public Task WithDefaultCtorShort([HttpTrigger] HttpRequest r) => throw new NotImplementedException();
                
                        [Function(nameof(WithDefaultCtor))]
                        public Task WithDefaultCtor([HttpTrigger()] HttpRequest r) => throw new NotImplementedException();
                
                        [Function(nameof(WithMethodsOnly))]
                        public Task WithMethodsOnly([HttpTrigger(Method)] HttpRequest r) => throw new NotImplementedException();
                
                        [Function(nameof(WithAuthMethodOnly))]
                        public Task WithAuthMethodOnly([HttpTrigger(DefaultLevel)] HttpRequest r) => throw new NotImplementedException();
                
                        [Function(nameof(WithAuthLevelAndMethods))]
                        public Task WithAuthLevelAndMethods([HttpTrigger(DefaultLevel, Method)] HttpRequest r) => throw new NotImplementedException();
                
                        [Function(nameof(WithAuthLevelAndMethodsAndRoutes))]
                        public Task WithAuthLevelAndMethodsAndRoutes([HttpTrigger(DefaultLevel, Method, Route = Prefix)] HttpRequest r) => throw new NotImplementedException();

                        [Function(nameof(WithAuthLevelAndMethodsAndRoutesInterpolated))]
                        public Task WithAuthLevelAndMethodsAndRoutesInterpolated([HttpTrigger(DefaultLevel, Method, Route = $"{Prefix}/other")] HttpRequest r) => throw new NotImplementedException();
                    }
                }
                """,
                languageVersion);
        }

        [Theory]
        [ClassData(typeof(SupportedLanguageTestCases))]
        public async Task Generate_WithDifferentTriggerCtors(LanguageVersion languageVersion)
        {
            await Test("""
                using System;
                using System.Collections.Generic;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.AspNetCore.Http;
                using System.Net.Http;
                using System.Threading;
                using System.Threading.Tasks;
                
                namespace Foo
                {
                    public sealed class HttpTriggersWithPlainArguments
                    {
                        [Function(nameof(WithMethodsOnly))]
                        public Task WithMethodsOnly([HttpTrigger("get", "post")] HttpRequest r) => throw new NotImplementedException();
                
                        [Function(nameof(WithAuthMethodOnly))]
                        public Task WithAuthMethodOnly([HttpTrigger(AuthorizationLevel.Function)] HttpRequest r) => throw new NotImplementedException();
                
                        [Function(nameof(WithAuthLevelAndMethods))]
                        public Task WithAuthLevelAndMethods([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest r) => throw new NotImplementedException();
                
                        [Function(nameof(WithAuthLevelAndMethodsAndRoutes))]
                        public Task WithAuthLevelAndMethodsAndRoutes([HttpTrigger(AuthorizationLevel.Function, "get", Route = "route")] HttpRequest r) => throw new NotImplementedException();
                    }
                }
                """,
                languageVersion);
        }

        private async Task Test(
            string sourceCode,
            LanguageVersion languageVersion,
            IReadOnlyCollection<Assembly>? additionalAssemblies = null,
            string? paramsNames = null,
            [CallerMemberName] string callerName = "")
        {
            await new SourceGeneratorValidator() { LanguageVersion = languageVersion }
                .WithGenerator(new PrecompiledFunctionMetadataProviderGenerator())
                .WithAssembly(
                    typeof(HttpTriggerAttribute).Assembly,
                    typeof(FunctionAttribute).Assembly,
                    typeof(HttpRequest).Assembly)
                .WithAssembly(additionalAssemblies)
                .WithInput(sourceCode)
                .Build()
                .AssertDiagnosticsOfGeneratedCode()
                .VerifySpecifiedFile(
                    BindingDeclarationEmiter.AssemblyMetadataFile,
                    parameters: paramsNames,
                    callerName: callerName);
        }
    }
}
