// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Azure.Functions.SdkGeneratorTests.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Sdk.Generators.MetadataGenerator;
using Xunit;

namespace Microsoft.Azure.Functions.SdkGeneratorTests.MetadataGeneratorTests
{
    public class ReferenceProjectTests
    {
        [Fact]
        public async Task WithDependentProjectWithFunctions()
        {
            var dependantProject = new SourceGeneratorValidator()
                .WithGenerator(new PrecompiledFunctionMetadataProviderGenerator(new[]
            {
                new BindingDeclarationEmiter()
            }))
                .WithAssembly(typeof(ServiceBusTriggerAttribute).Assembly)
                .WithInput($$"""
                using System;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;

                namespace OtherAssemblyNamespace
                {
                    public class OtherAssemblyClass
                    {
                        [Function(nameof(OtherAssemblyFunction))]
                        public Task OtherAssemblyFunction(
                            [ServiceBusTrigger("queueName")] string value)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """)
                .Build();

            await new SourceGeneratorValidator()
                .WithGenerator(new PrecompiledFunctionMetadataProviderGenerator(new[]
                {
                    new BindingDeclarationEmiter()
                }))
                .WithAssembly(
                    typeof(HttpTriggerAttribute).Assembly,
                    typeof(HttpResponseData).Assembly)
                .WithAssembly(dependantProject)
                .WithInput("""
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
                """)
                .Build()
                .VerifyOutput();
        }

        [Fact]
        public async Task WithMultipleIncludeReferences()
        {
            var dependantProject = new SourceGeneratorValidator()
                .WithGenerator(new PrecompiledFunctionMetadataProviderGenerator(new[]
                {
                    new BindingDeclarationEmiter()
                }))
                .WithAssembly(typeof(ServiceBusTriggerAttribute).Assembly)
                .WithInput($$"""
                using System;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;

                namespace OtherAssemblyNamespace_1
                {
                    public class OtherAssemblyClass_1
                    {
                        [Function(nameof(OtherAssemblyFunction_1))]
                        public Task OtherAssemblyFunction_1(
                            [ServiceBusTrigger("queueName")] string value)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """)
                .Build();

            var middleLevelProject = new SourceGeneratorValidator()
                .WithGenerator(new PrecompiledFunctionMetadataProviderGenerator(new[]
                {
                    new BindingDeclarationEmiter()
                }))
                .WithAssembly(typeof(ServiceBusTriggerAttribute).Assembly)
                .WithAssembly(dependantProject)
                .WithInput($$"""
                using System;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;

                namespace OtherAssemblyNamespace_2
                {
                    public class OtherAssemblyClass_2
                    {
                        [Function(nameof(OtherAssemblyFunction_2))]
                        public Task OtherAssemblyFunction_2(
                            [ServiceBusTrigger("queueName")] string value)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """)
                .Build();

            await new SourceGeneratorValidator()
                .WithGenerator(new PrecompiledFunctionMetadataProviderGenerator())
                .WithAssembly(
                    typeof(HttpTriggerAttribute).Assembly,
                    typeof(HttpResponseData).Assembly)
                .WithAssembly(dependantProject)
                .WithAssembly(middleLevelProject)
                .WithInput("""
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
                """)
                .Build()
                .VerifyOutput();
        }
    }
}
