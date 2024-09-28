// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.SdkGeneratorTests.Helpers;
using Microsoft.Azure.Functions.Worker.Sdk.Generators;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Microsoft.Azure.Functions.SdkGeneratorTests
{
    public partial class FunctionMetadataProviderGeneratorTests
    {
        public class HttpTriggerTests
        {
            [Theory]
            [InlineData(LanguageVersion.CSharp7_3)]
            [InlineData(LanguageVersion.CSharp8)]
            [InlineData(LanguageVersion.CSharp9)]
            [InlineData(LanguageVersion.CSharp10)]
            [InlineData(LanguageVersion.CSharp11)]
            [InlineData(LanguageVersion.Latest)]
            public async Task GenerateSimpleHttpTriggerMetadataTest(LanguageVersion languageVersion)
            {
                string inputCode = """
                using System;
                using System.Collections.Generic;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;

                namespace FunctionApp
                {
                    public static class HttpTriggerSimple
                    {
                        [Function(nameof(HttpTriggerSimple))]
                        public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req, FunctionContext executionContext)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """;

                await Test(
                    inputCode,
                    languageVersion);

            }

            [Theory]
            [InlineData(LanguageVersion.CSharp7_3)]
            [InlineData(LanguageVersion.CSharp8)]
            [InlineData(LanguageVersion.CSharp9)]
            [InlineData(LanguageVersion.CSharp10)]
            [InlineData(LanguageVersion.CSharp11)]
            [InlineData(LanguageVersion.Latest)]
            public async void BasicHttpFunctionWithNoResponse(LanguageVersion languageVersion)
            {
                string inputCode = """
                using System;
                using System.Collections.Generic;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;

                namespace FunctionApp
                {
                    public static class HttpTriggerSimple
                    {
                        [Function("HttpTrigger")]
                        public static void HttpTrigger([HttpTrigger(AuthorizationLevel.Admin, "get", "post", Route = "/api2")] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """;

                await Test(
                     inputCode,
                     languageVersion);
            }

            [Theory]
            [InlineData(LanguageVersion.CSharp7_3)]
            [InlineData(LanguageVersion.CSharp8)]
            [InlineData(LanguageVersion.CSharp9)]
            [InlineData(LanguageVersion.CSharp10)]
            [InlineData(LanguageVersion.CSharp11)]
            [InlineData(LanguageVersion.Latest)]
            public async void ReturnTypeJustHttp(LanguageVersion languageVersion)
            {
                string inputCode = """
                using System;
                using System.Collections.Generic;
                using Microsoft.Azure.Functions.Worker.Http;
                using Microsoft.Azure.Functions.Worker;

                namespace Foo
                {
                    public class HttpTriggerSimple
                    {
                        [Function("JustHtt")]
                        public JustHttp Justhtt([HttpTrigger("get")] string req)
                        {
                            throw new NotImplementedException();
                        }

                        public class JustHttp
                        {
                            public HttpResponseData httpResponseProp { get; set; }
                        }
                    }
                }
                """;

                // override the namespace value for generated types using msbuild property.
                var buildPropertiesDict = new Dictionary<string, string>()
                {
                    {  Constants.BuildProperties.GeneratedCodeNamespace, "MyCompany.MyProject.MyApp"}
                };

                await Test(
                    inputCode,
                    languageVersion,
                    buildPropertiesDictionary: buildPropertiesDict);
            }

            [Theory]
            [InlineData(LanguageVersion.CSharp7_3)]
            [InlineData(LanguageVersion.CSharp8)]
            [InlineData(LanguageVersion.CSharp9)]
            [InlineData(LanguageVersion.CSharp10)]
            [InlineData(LanguageVersion.CSharp11)]
            [InlineData(LanguageVersion.Latest)]
            public async void NonStaticVoidOrTaskReturnType(LanguageVersion languageVersion)
            {
                string inputCode = """
                using System;
                using System.Collections.Generic;
                using Microsoft.Azure.Functions.Worker.Http;
                using Microsoft.Azure.Functions.Worker;
                using System.Threading;
                using System.Threading.Tasks;

                namespace Foo
                {
                    public sealed class HttpTriggers
                    {
                        [Function("Function1")]
                        public void FunctionWithVoidReturnType([HttpTrigger("get")] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }
                        [Function("Function2")]
                        public Task FunctionWithTaskReturnType([HttpTrigger("get")] HttpRequestData req)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """;

                // override the namespace value for generated types using msbuild property.
                var buildPropertiesDict = new Dictionary<string, string>()
                {
                    {  Constants.BuildProperties.GeneratedCodeNamespace, "MyCompany.MyProject.MyApp"}
                };

                await Test(
                    inputCode,
                    languageVersion,
                    buildPropertiesDictionary: buildPropertiesDict);
            }

            private async Task Test(
                string sourceCode,
                LanguageVersion languageVersion,
                IDictionary<string, string>? buildPropertiesDictionary = null,
                [CallerMemberName] string callerName = "")
            {
                await new SourceGeneratorValidator() { LanguageVersion = languageVersion }
                    .Configure(x => x.With(buildPropertiesDictionary))
                    .WithGenerator(new FunctionMetadataProviderGenerator())
                    .WithAssembly(
                        Assembly.LoadFrom("Microsoft.Azure.Functions.Worker.Extensions.Abstractions.dll"),
                        Assembly.LoadFrom("Microsoft.Azure.Functions.Worker.Extensions.Http.dll"),
                        typeof(HostBuilder).Assembly,
                        typeof(DefaultServiceProviderFactory).Assembly,
                        typeof(IHost).Assembly,
                        typeof(IServiceCollection).Assembly)
                    .WithInput(sourceCode)
                    .Build()
                    .AssertDiagnosticsOfGeneratedCode()
                    .VerifyOutput();
            }
        }
    }
}
