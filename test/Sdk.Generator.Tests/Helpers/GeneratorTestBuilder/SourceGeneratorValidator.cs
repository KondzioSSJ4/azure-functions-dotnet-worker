using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Microsoft.Azure.Functions.SdkGeneratorTests.Helpers
{
    internal sealed class SourceGeneratorValidator
    {
        private static readonly string _dotNetAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        private readonly List<ISourceGenerator> _generators = new();
        private readonly List<string> _inputs = new();
        private readonly LanguageVersion _languageVersion;
        private readonly HashSet<Assembly> _assemblies = new()
        {
            typeof(FunctionAttribute).Assembly,
            typeof(Task).Assembly,
            typeof(WorkerExtensionStartupAttribute).Assembly,
            typeof(HostBuilder).Assembly
        };

        private readonly List<string> _ignoredErrors = new()
        {
            "CS5001" // Program does not contain a static 'Main' method suitable for an entry point
        };

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Latest;
        public SourceGeneratorConfigProvider Config { get; } = new();

        public SourceGeneratorValidator WithGenerator(IIncrementalGenerator generator)
        {
            _generators.Add(GeneratorExtensions.AsSourceGenerator(generator));
            return this;
        }

        public SourceGeneratorValidator WithGenerator(ISourceGenerator generator)
        {
            _generators.Add(generator);
            return this;
        }

        public SourceGeneratorValidator WithAssemblies(params Assembly[] assemblies)
        {
            foreach (var item in assemblies)
            {
                _assemblies.Add(item);
            }

            return this;
        }

        public SourceGeneratorValidator WithAssembly(params SourceGeneratorResult[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var path = assembly.GetTemporalAssemblyFile();
                _assemblies.Add(Assembly.LoadFrom(path));
            }

            return this;
        }

        public SourceGeneratorValidator WithInput(params string[] inputs)
        {
            _inputs.AddRange(inputs);
            return this;
        }

        public SourceGeneratorResult Build(
            CancellationToken cancellationToken = default)
        {
            var parseOptions = new CSharpParseOptions(LanguageVersion);
            var syntaxTrees = _inputs.Select(x => CSharpSyntaxTree.ParseText(x, parseOptions)).ToArray();

            var metadata = GetAllAssemblies(_assemblies)
                .Distinct()
                .Select(l => MetadataReference.CreateFromFile(l))
                .ToArray();

            var compilation = CSharpCompilation.Create(
                "TestProject",
                syntaxTrees,
                metadata);

            AssertDiagnostics(compilation);

            var analyzerOptions = Config.ToProvider();
            var driver = CSharpGeneratorDriver.Create(_generators)
                .WithUpdatedAnalyzerConfigOptions(analyzerOptions);

            var generateResult = driver.RunGenerators(compilation, cancellationToken);

            return new SourceGeneratorResult(
                compilation,
                driver,
                parseOptions,
                generateResult.GetRunResult(),
                _ignoredErrors);
        }

        private void AssertDiagnostics(CSharpCompilation compilation)
        {
            var errors = compilation.GetDiagnostics()
                .Where(d => d.Severity >= DiagnosticSeverity.Error
                    && !_ignoredErrors.Contains(d.Id))
                .ToArray();

            Assert.Empty(errors);
        }

        private static IEnumerable<string> GetAllAssemblies(
            IEnumerable<Assembly> assemblies,
            HashSet<string>? alreadyConsumed = null)
        {
            alreadyConsumed ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in assemblies)
            {
                if (!alreadyConsumed.Add(item.Location))
                {
                    continue;
                }

                yield return item.Location;

                foreach (var nestedAssembly in GetAllAssemblies(item
                    .GetReferencedAssemblies()
                    .Select(x => Assembly.Load(x)), alreadyConsumed))
                {
                    yield return nestedAssembly;
                }
            }

            foreach (var item in GetBaseCompilationAssemblies())
            {
                if (alreadyConsumed.Add(item))
                {
                    yield return item;
                }
            }
        }

        private static IEnumerable<string> GetBaseCompilationAssemblies()
        {
            yield return Path.Combine(_dotNetAssemblyPath, "netstandard.dll");
            yield return Path.Combine(_dotNetAssemblyPath, "System.dll");
            yield return Path.Combine(_dotNetAssemblyPath, "System.Core.dll");
            yield return Path.Combine(_dotNetAssemblyPath, "System.Private.CoreLib.dll");
            yield return Path.Combine(_dotNetAssemblyPath, "System.Runtime.dll");
        }
    }
}
