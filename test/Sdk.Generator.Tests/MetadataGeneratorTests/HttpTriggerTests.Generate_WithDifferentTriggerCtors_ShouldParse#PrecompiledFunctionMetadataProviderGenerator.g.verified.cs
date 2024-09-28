//HintName: PrecompiledFunctionMetadataProviderGenerator.g.cs
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Core.FunctionMetadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AzureFunctionInternals.TestProject
{
    public sealed class PrecompiledFunctionMetadataProvider : global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadataProvider
    {
        public global::System.Threading.Tasks.Task<global::System.Collections.Immutable.ImmutableArray<global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>> GetFunctionMetadataAsync(
            string directory)
        {
            var results = global::System.Collections.Immutable.ImmutableArray.Create<global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>(new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata[]
            {
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "575951168",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "FunctionExactName",
                   entryPoint: "FunctionExactNameMethod",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "r",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: global::System.Collections.Immutable.ImmutableDictionary<string,string>.Empty) 
                   })                ,
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "1404824785",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithDefaultCtorShort",
                   entryPoint: "WithDefaultCtorShort",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "r",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: global::System.Collections.Immutable.ImmutableDictionary<string,string>.Empty) 
                   })                ,
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "3453661689",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithDefaultCtor",
                   entryPoint: "WithDefaultCtor",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "r",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: global::System.Collections.Immutable.ImmutableDictionary<string,string>.Empty) 
                   })                ,
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "225417419",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithMethodsOnly",
                   entryPoint: "WithMethodsOnly",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In"",""methods"":""[\u0022get\u0022,\u0022post\u0022]""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "r",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: new global::System.Collections.Generic.Dictionary<string, string>(1)
                          {
                              { "methods", "["get","post"]" }
                          }) 
                   })                ,
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "319677613",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithAuthMethodOnly",
                   entryPoint: "WithAuthMethodOnly",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In"",""authLevel"":""Function""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "r",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: new global::System.Collections.Generic.Dictionary<string, string>(1)
                          {
                              { "authLevel", "Function" }
                          }) 
                   })                ,
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "2714009341",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithAuthLevelAndMethods",
                   entryPoint: "WithAuthLevelAndMethods",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In"",""authLevel"":""Function"",""methods"":""[\u0022get\u0022]""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "r",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: new global::System.Collections.Generic.Dictionary<string, string>(2)
                          {
                              { "authLevel", "Function" },
                              { "methods", "["get"]" }
                          }) 
                   })                ,
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "295470615",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithAuthLevelAndMethodsAndRoutes",
                   entryPoint: "WithAuthLevelAndMethodsAndRoutes",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In"",""authLevel"":""Function"",""methods"":""[\u0022get\u0022]"",""route"":""route""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "r",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: new global::System.Collections.Generic.Dictionary<string, string>(3)
                          {
                              { "authLevel", "Function" },
                              { "methods", "["get"]" },
                              { "route", "route" }
                          }) 
                   })                ,
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "575951168",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "FunctionExactName",
                   entryPoint: "FunctionExactNameMethod",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "r",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: global::System.Collections.Immutable.ImmutableDictionary<string,string>.Empty) 
                   })                ,
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "1404824785",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithDefaultCtorShort",
                   entryPoint: "WithDefaultCtorShort",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "r",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: global::System.Collections.Immutable.ImmutableDictionary<string,string>.Empty) 
                   })                ,
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "3453661689",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithDefaultCtor",
                   entryPoint: "WithDefaultCtor",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "r",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: global::System.Collections.Immutable.ImmutableDictionary<string,string>.Empty) 
                   })                ,
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "225417419",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithMethodsOnly",
                   entryPoint: "WithMethodsOnly",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In"",""methods"":""[\u0022post\u0022]""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "r",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: new global::System.Collections.Generic.Dictionary<string, string>(1)
                          {
                              { "methods", "["post"]" }
                          }) 
                   })                ,
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "319677613",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithAuthMethodOnly",
                   entryPoint: "WithAuthMethodOnly",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In"",""authLevel"":""Function""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "r",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: new global::System.Collections.Generic.Dictionary<string, string>(1)
                          {
                              { "authLevel", "Function" }
                          }) 
                   })                ,
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "2714009341",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithAuthLevelAndMethods",
                   entryPoint: "WithAuthLevelAndMethods",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In"",""authLevel"":""Function"",""methods"":""[\u0022post\u0022]""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "r",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: new global::System.Collections.Generic.Dictionary<string, string>(2)
                          {
                              { "authLevel", "Function" },
                              { "methods", "["post"]" }
                          }) 
                   })                ,
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "295470615",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithAuthLevelAndMethodsAndRoutes",
                   entryPoint: "WithAuthLevelAndMethodsAndRoutes",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In"",""authLevel"":""Function"",""methods"":""[\u0022post\u0022]"",""route"":""function-prefix""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "r",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: new global::System.Collections.Generic.Dictionary<string, string>(3)
                          {
                              { "authLevel", "Function" },
                              { "methods", "["post"]" },
                              { "route", "function-prefix" }
                          }) 
                   })                ,
               new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.SourceGeneratedFunctionMetadata(
                   functionId: "818281373",
                   isProxy: false,
                   language: "dotnet-isolated",
                   managedDependencyEnabled: false,
                   name: "WithAuthLevelAndMethodsAndRoutesInterpolated",
                   entryPoint: "WithAuthLevelAndMethodsAndRoutesInterpolated",
                   scriptFile: "TestProject.dll",
                   retry: null,
                   rawBindings: new global::System.Collections.Generic.List<string>(1)
                   { 
                       @"{""name"":""r"",""type"":""httpTrigger"",""direction"":""In"",""authLevel"":""Function"",""methods"":""[\u0022post\u0022]"",""route"":""function-prefix/other""}"
                   },
                   generatedBindings: new global::System.Collections.Generic.List<IGeneratedBinding>(1)
                   { 
                       new global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.DefaultGeneratedBinding(
                          name: "r",
                          direction: global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.FunctionBindingDirection.In,
                          bindingType: "httpTrigger",
                          dataType: null /* TODO */,
                          properties: new global::System.Collections.Generic.Dictionary<string, string>(3)
                          {
                              { "authLevel", "Function" },
                              { "methods", "["post"]" },
                              { "route", "function-prefix/other" }
                          }) 
                   })                

            });

            return global::System.Threading.Tasks.Task.FromResult<
                global::System.Collections.Immutable.ImmutableArray<
                    global::Microsoft.Azure.Functions.Worker.Core.FunctionMetadata.IFunctionMetadata>>(results);
        }
    }
}