// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.SdkGeneratorTests.Helpers;
using Microsoft.Azure.Functions.Worker.Sdk.Generators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Microsoft.Azure.Functions.SdkGeneratorTests
{
    public partial class FunctionExecutorGeneratorTests
    {
        public class DependentAssemblyTest
        {
            [Fact]
            public async Task FunctionsFromDependentAssembly()
            {
                await new SourceGeneratorValidator()
                .WithGenerator(new FunctionExecutorGenerator())
                .WithAssembly(
                    Assembly.LoadFrom("Microsoft.Azure.Functions.Worker.Extensions.Abstractions.dll"),
                    Assembly.LoadFrom("Microsoft.Azure.Functions.Worker.Extensions.Http.dll"),
                    typeof(HostBuilder).Assembly,
                    typeof(DefaultServiceProviderFactory).Assembly,
                    typeof(IHost).Assembly,
                    typeof(IServiceCollection).Assembly,
                    Assembly.LoadFrom("DependentAssemblyWithFunctions.dll"))
                .WithInput("""
                        using System;
                        using Microsoft.Azure.Functions.Worker;
                        using Microsoft.Azure.Functions.Worker.Http;
                        namespace MyCompany
                        {
                            public class MyHttpTriggers
                            {
                                [Function("FunctionA")]
                                public HttpResponseData Foo([HttpTrigger(AuthorizationLevel.User, "get")] HttpRequestData r, FunctionContext c)
                                {
                                    return r.CreateResponse(System.Net.HttpStatusCode.OK);
                                }
                            }
                        }
                        """)
                .Build()
                .AssertDiagnosticsOfGeneratedCode()
                .VerifyOutput();
            }
        }
    }
}
