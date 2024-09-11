using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.SdkGeneratorTests.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Sdk.Generators.FunctionActivator;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Microsoft.Azure.Functions.SdkGeneratorTests.FunctionActivator
{
    public class FunctionActivatorTests
    {
        [Fact]
        public async Task Generate_WithStaticClass_ShouldNotGenerateActivator()
        {
            await Test("""
                using System;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;

                namespace MyCompany
                {
                    public static class MyHttpTriggers
                    {
                        [Function("FunctionA")]
                        public static Task Foo([HttpTrigger(AuthorizationLevel.User, "get")] HttpRequestData r)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """);
        }

        [Fact]
        public async Task Generate_WithStaticMethod_ShouldNotGenerateActivator()
        {
            await Test("""
                using System;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                namespace MyCompany
                {
                    public class MyHttpTriggers
                    {
                        [Function("FunctionA")]
                        public static Task Foo([HttpTrigger(AuthorizationLevel.User, "get")] HttpRequestData r)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """);
        }

        [Fact]
        public async Task Generate_WithoutCtorSyntax_ShouldGenerateActivator()
        {
            await Test("""
                using System;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                namespace MyCompany
                {
                    public class MyHttpTriggers
                    {
                        [Function("FunctionA")]
                        public Task Foo([HttpTrigger(AuthorizationLevel.User, "get")] HttpRequestData r)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """);
        }

        [Fact]
        public async Task Generate_WithoutDefaultCtor_ShouldGenerateActivatorWithInjection()
        {
            await Test("""
                using System;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                namespace MyCompany
                {
                    public class MyHttpTriggers
                    {
                        public MyHttpTriggers(InjectedClass val){
                            
                        }

                        [Function("FunctionA")]
                        public Task Foo([HttpTrigger(AuthorizationLevel.User, "get")] HttpRequestData r)
                        {
                            throw new NotImplementedException();
                        }

                        public class InjectedClass { }
                    }
                }
                """);
        }

        [Fact]
        public async Task Generate_WithManyCtors_ShouldThrowDiagnosticIssue()
        {
            await Test("""
                using System;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                namespace MyCompany
                {
                    public class MyHttpTriggers
                    {
                        public MyHttpTriggers(InjectedClass val){
                            
                        }

                        public MyHttpTriggers(OtherInjectedClass val){
                            
                        }

                        [Function("FunctionA")]
                        public Task Foo([HttpTrigger(AuthorizationLevel.User, "get")] HttpRequestData r)
                        {
                            throw new NotImplementedException();
                        }
                    }
                
                    public class InjectedClass { }
                
                    public class OtherInjectedClass { }
                }
                """,
                verifyDiagnostics: true);
        }

        [Fact]
        public async Task Generate_WithRecord_ShouldInject()
        {
            await Test("""
                using System;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                namespace MyCompany
                {
                    public record MyHttpTriggers(InjectedClass Val)
                    {
                        [Function("FunctionA")]
                        public Task Foo([HttpTrigger(AuthorizationLevel.User, "get")] HttpRequestData r)
                        {
                            throw new NotImplementedException();
                        }
                    }
                
                    public class InjectedClass { }
                
                    public class OtherInjectedClass { }
                }
                """,
                languageVersion: LanguageVersion.CSharp10);
        }

        [Fact]
        public async Task Generate_WithManyMethods_ShouldGenerateSingleInjection()
        {
            await Test("""
                using System;
                using System.Threading.Tasks;
                using Microsoft.Azure.Functions.Worker;
                using Microsoft.Azure.Functions.Worker.Http;
                namespace MyCompany
                {
                    public class MyHttpTriggers
                    {
                        public MyHttpTriggers(InjectedClass val){
                            
                        }

                        [Function("FunctionA")]
                        public Task GetMethod([HttpTrigger(AuthorizationLevel.User, "get")] HttpRequestData r)
                        {
                            throw new NotImplementedException();
                        }

                        [Function("FunctionB")]
                        public Task PostMethod([HttpTrigger(AuthorizationLevel.User, "post")] HttpRequestData r)
                        {
                            throw new NotImplementedException();
                        }
                    }
                
                    public class InjectedClass { }
                }
                """,
                languageVersion: LanguageVersion.CSharp10);
        }

        private async Task Test(
            string sourceCode,
            bool verifyDiagnostics = false,
            LanguageVersion? languageVersion = null,
            [CallerMemberName] string callerName = "")
        {
            await new FunctionActivatorGenerator()
                .RunAndVerify(
                    sourceCode,
                    new[]
                    {
                        typeof(HttpTriggerAttribute).Assembly,
                        typeof(FunctionAttribute).Assembly
                    },
                    verifyDiagnostics: verifyDiagnostics,
                    languageVersion: languageVersion,
                    callerName: callerName);
        }
    }
}
