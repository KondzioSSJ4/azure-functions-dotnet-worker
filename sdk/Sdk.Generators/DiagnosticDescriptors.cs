// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators
{
    internal class DiagnosticDescriptors
    {
        private static DiagnosticDescriptor Create(string id, string title, string messageFormat, string category, DiagnosticSeverity severity)
        {
            var helpLink = $"https://aka.ms/azfw-rules?ruleid={id}";

            return new DiagnosticDescriptor(id, title, messageFormat, category, severity, isEnabledByDefault: true, helpLinkUri: helpLink);
        }

        public static DiagnosticDescriptor IncorrectBaseType { get; }
                = Create(id: "AZFW0003",
                    title: "Invalid base class for extension startup type.",
                    messageFormat: "'{0}' must derive from '{1}'.",
                    category: "Startup",
                    severity: DiagnosticSeverity.Error);

        public static DiagnosticDescriptor ConstructorMissing { get; }
                = Create(id: "AZFW0004",
                    title: "Extension startup type is missing parameterless constructor.",
                    messageFormat: "'{0}' class must have a public parameterless constructor.",
                    category: "Startup",
                    severity: DiagnosticSeverity.Error);

        public static DiagnosticDescriptor MultipleBindingsGroupedTogether { get; }
                = Create(id: "AZFW0005",
                    title: "Multiple bindings are grouped together on one property, method, or parameter syntax.",
                    messageFormat: "{0} '{1}' must have only one binding attribute.",
                    category: "FunctionMetadataGeneration",
                    severity: DiagnosticSeverity.Error);

        public static DiagnosticDescriptor SymbolNotFound { get; }
                = Create(id: "AZFW0006",
                    title: "Symbol could not be found in user compilation.",
                    messageFormat: "The symbol '{0}' could not be found.",
                    category: "FunctionMetadataGeneration",
                    severity: DiagnosticSeverity.Warning);

        public static DiagnosticDescriptor MultipleHttpResponseTypes { get; }
                  = Create(id: "AZFW0007",
                    title: "Symbol could not be found in user compilation.",
                    messageFormat: "Found multiple HTTP Response types (properties with HttpResultAttribute or properties of type HttpResponseData) defined in return type '{0}'. Only one HTTP response binding type is supported in your return type definition.",
                    category: "FunctionMetadataGeneration",
                    severity: DiagnosticSeverity.Error);

        public static DiagnosticDescriptor InvalidCardinality { get; }
                  = Create(id: "AZFW0008",
                    title: "Input or trigger binding cardinality is invalid.",
                    messageFormat: "The cardinality of the input or trigger binding on parameter '{0}' is invalid. IsBatched may be used incorrectly.",
                    category: "FunctionMetadataGeneration",
                    severity: DiagnosticSeverity.Error);

        public static DiagnosticDescriptor InvalidRetryOptions { get; }
                    = Create(id: "AZFW0012",
                    title: "Invalid operation with a retry attribute.",
                    messageFormat: "Invalid use of a retry attribute. Check that the attribute is used on a trigger that supports function-level retry.",
                    category: "FunctionMetadataGeneration",
                    severity: DiagnosticSeverity.Error);
        public static DiagnosticDescriptor InvalidBindingAttributeArgument { get; }
                    = Create(id: "AZFW0013",
                    title: "Invalid argument in binding attribute.",
                    messageFormat: "Invalid argument passed in binding attribute. Check that the argument is not null.",
                    category: "FunctionMetadataGeneration",
                    severity: DiagnosticSeverity.Error);

        public static DiagnosticDescriptor InconclusiveAttribute { get; }
            = Create(id: "AZFW0014",
                title: "Multiple attributes assigned to method or property",
                messageFormat: "Multiple attributes ({0}) assigned to method or property. Remove duplicated attributes",
                category: "FunctionMetadataGeneration",
                severity: DiagnosticSeverity.Error);

        public static DiagnosticDescriptor InconclusiveCtor { get; }
            = Create(id: "AZFW0015",
                title: "Function have many constructors",
                messageFormat: "Class or record have more than 1 constructor",
                category: "FunctionMetadataGeneration",
                severity: DiagnosticSeverity.Error);

        public static DiagnosticDescriptor InvalidInputTriggerCount { get; }
            = Create(id: "AZFW0016",
                title: "Function must have 1 and only 1 trigger",
                messageFormat: "Functions must have 1 trigger, like HttpTriggerAttribute or ServiceBusTriggerAttribute e.c.t. Your function have {0} triggers",
                category: "FunctionMetadataGeneration",
                severity: DiagnosticSeverity.Error);

        public static DiagnosticDescriptor AsyncVoidIsNotAllowed { get; }
            = Create(id: "AZFW0018",
                title: "Async void methods are not allowed for Azure Functions",
                messageFormat: "Functions must have handler of the call, like Task to knows when it was finished.",
                category: "FunctionMetadataGeneration",
                severity: DiagnosticSeverity.Error);

        public static DiagnosticDescriptor InvalidBindingType { get; }
            = Create(id: "AZFW0019",
                title: "Invalid binding type",
                messageFormat: "Binding type not allow {0} as {1} type",
                category: "FunctionMetadataGeneration",
                severity: DiagnosticSeverity.Error);

        public static DiagnosticDescriptor InconclusiveOutputBinding { get; }
            = Create(id: "AZFW0020",
                title: "Inconclusive output binding",
                messageFormat: """
                Output binding must return single model output or model with nested outputs.
                Not allowed:
                    - Method returning result without output definition
                    - Output definition exists but return void or Task
                    - IAsyncEnumerable or IEnumerable with nested model bindings
                    - Method model output by attribute with nested outputs in model
                """,
                category: "FunctionMetadataGeneration",
                severity: DiagnosticSeverity.Error);

        public static DiagnosticDescriptor InvalidRetryArgument { get; }
            = Create(id: "AZFW0021",
                title: "Invalid retry provided",
                messageFormat: """
                Invalid retry, {0} must be {1}.
                """,
                category: "FunctionMetadataGeneration",
                severity: DiagnosticSeverity.Error);

        // TODO - ideas:
        // Warning when there is `Output` attribute with void/Task
        // Output tuples are not allowed
    }
}
