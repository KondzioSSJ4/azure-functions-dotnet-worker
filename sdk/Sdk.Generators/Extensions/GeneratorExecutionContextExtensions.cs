﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators
{
    internal static class GeneratorExecutionContextExtensions
    {
        /// <summary>
        /// Returns true if the source generator is running in the context of an "Azure Function" project.
        /// </summary>
        internal static bool IsRunningInAzureFunctionProject(this GeneratorExecutionContext context)
        {
            return context.AnalyzerConfigOptions.IsRunningInAzureFunctionProject();
        }

        internal static bool IsRunningInAzureFunctionProject(this AnalyzerConfigOptionsProvider? provider)
        {
            if (provider?.GlobalOptions?.TryGetValue(Constants.BuildProperties.FunctionsExecutionModel, out var value) ?? false)
            {
                return string.Equals(value, Constants.ExecutionModel.Isolated);
            }

            return false;
        }

        internal static bool ShouldExecuteGeneration(this GeneratorExecutionContext context)
        {
            if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
                    Constants.BuildProperties.EnableMetadataSourceGen, out var sourceGenSwitch))
            {
                return false;
            }

            bool.TryParse(sourceGenSwitch, out bool enableSourceGen);
            return enableSourceGen;
        }

        internal static bool ShouldExecuteGeneration(this AnalyzerConfigOptionsProvider? provider)
        {
            if (provider?.GlobalOptions?.TryGetValue(Constants.BuildProperties.EnableMetadataSourceGen, out var sourceGenSwitch) ?? false)
            {
                bool.TryParse(sourceGenSwitch, out bool enableSourceGen);
                return enableSourceGen;
            }

            return false;
        }

        internal static bool ShouldIncludeAutoGeneratedAttributes(this GeneratorExecutionContext context)
        {
            if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
                    Constants.BuildProperties.AutoRegisterGeneratedMetadataProvider, out var autoRegisterSwitch))
            {
                return false;
            }

            bool.TryParse(autoRegisterSwitch, out bool enableRegistration);
            return enableRegistration;
        }
    }
}
