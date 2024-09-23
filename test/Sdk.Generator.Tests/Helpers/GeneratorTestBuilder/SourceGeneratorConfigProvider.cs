using System.Collections.Generic;

namespace Microsoft.Azure.Functions.SdkGeneratorTests.Helpers
{
    internal sealed class SourceGeneratorConfigProvider
    {
        private readonly Dictionary<string, string> _config = new()
        {
            ["is_global"] = true.ToString(),
            ["build_property.FunctionsEnableExecutorSourceGen"] = true.ToString(),
            ["build_property.FunctionsEnableMetadataSourceGen"] = true.ToString(),
            ["build_property.FunctionsGeneratedCodeNamespace"] = "TestProject",
            ["build_property.FunctionsExecutionModel"] = "isolated"
        };

        public SourceGeneratorConfigProvider WithNamespace(string generatedCodeNamespace)
        {
            _config["build_property.FunctionsGeneratedCodeNamespace"] = generatedCodeNamespace;
            return this;
        }

        public SourceGeneratorConfigProvider AsAzureFunctionProject(bool isAzureFunction = true)
        {
            const string Key = "build_property.FunctionsExecutionModel";
            if (isAzureFunction)
            {
                _config[Key] = "isolated";
            }
            else
            {
                _config.Remove(Key);
            }

            return this;
        }

        public SourceGeneratorConfigProvider With(string key, string value)
        {
            _config[key] = value;
            return this;
        }

        public SourceGeneratorConfigProvider With(IEnumerable<KeyValuePair<string, string>> values)
        {
            foreach (var pair in values)
            {
                _config[pair.Key] = pair.Value;
            }

            return this;
        }

        public AnalyzerConfigOptions ToProvider() => new(_config);
    }
}
