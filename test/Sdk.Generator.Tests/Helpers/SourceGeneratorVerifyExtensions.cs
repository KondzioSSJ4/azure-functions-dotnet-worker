using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Hosting;
using VerifyTests;
using VerifyXunit;
using Xunit;

namespace Microsoft.Azure.Functions.SdkGeneratorTests.Helpers
{
    internal static class SourceGeneratorVerifyExtensions
    {
        private const LanguageVersion _defaultLanguageVersion = LanguageVersion.CSharp7_3;

        private static readonly string _dotNetAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        private static readonly TimeSpan _executionMaxTime = TimeSpan.FromSeconds(30);

        public static async Task RunAndVerify(
            this GeneratorDriver driver,
            string inputSource,
            IEnumerable<Assembly>? extensionAssemblyReferences = null,
            IDictionary<string, string>? buildPropertiesDictionary = null,
            string? generatedCodeNamespace = null,
            LanguageVersion? languageVersion = null,
            bool runInsideAzureFunctionProject = true,
            string? paramsNames = "",
            [CallerFilePath] string callerFileName = "",
            [CallerMemberName] string callerName = "")
        {
            using var cts = new CancellationTokenSource();
#if !DEBUG
            cts.CancelAfter(_executionMaxTime);
#endif

            var compilation = CreateCompilation(inputSource, extensionAssemblyReferences, languageVersion);
            if (!AssertDiagnostics(compilation))
            {
                return;
            }

            var config = CreateAnalyzerOptions(
                buildPropertiesDictionary,
                generatedCodeNamespace,
                runInsideAzureFunctionProject);

            cts.Token.ThrowIfCancellationRequested();
            driver = driver
                .WithUpdatedAnalyzerConfigOptions(config);

            var generateResult = driver.RunGenerators(compilation, cts.Token);
            cts.Token.ThrowIfCancellationRequested();

            await VerifyGeneratedCode(generateResult, callerFileName, callerName, paramsNames);
            await AssertDiagnosticsOfGeneratedCode(languageVersion, compilation, generateResult, cts.Token);
        }

        public static Task RunAndVerify(
            this IIncrementalGenerator sourceGenerator,
            string inputSource,
            IEnumerable<Assembly>? extensionAssemblyReferences = null,
            IDictionary<string, string>? buildPropertiesDictionary = null,
            string? generatedCodeNamespace = null,
            LanguageVersion? languageVersion = null,
            bool runInsideAzureFunctionProject = true,
            string? paramsNames = "",
            [CallerFilePath] string callerFileName = "",
            [CallerMemberName] string callerName = "")
        {
            return CSharpGeneratorDriver.Create(sourceGenerator)
                .RunAndVerify(
                    inputSource,
                    extensionAssemblyReferences,
                    buildPropertiesDictionary,
                    generatedCodeNamespace,
                    languageVersion,
                    runInsideAzureFunctionProject,
                    paramsNames,
                    callerFileName,
                    callerName);
        }

        public static Task RunAndVerify(
            this ISourceGenerator sourceGenerator,
            string inputSource,
            IEnumerable<Assembly>? extensionAssemblyReferences = null,
            IDictionary<string, string>? buildPropertiesDictionary = null,
            string? generatedCodeNamespace = null,
            LanguageVersion? languageVersion = null,
            bool runInsideAzureFunctionProject = true,
            string? paramsNames = "",
            [CallerFilePath] string callerFileName = "",
            [CallerMemberName] string callerName = "")
        {
            return CSharpGeneratorDriver.Create(sourceGenerator)
                .RunAndVerify(
                    inputSource,
                    extensionAssemblyReferences,
                    buildPropertiesDictionary,
                    generatedCodeNamespace,
                    languageVersion,
                    runInsideAzureFunctionProject,
                    paramsNames,
                    callerFileName,
                    callerName);
        }

        private static async Task AssertDiagnosticsOfGeneratedCode(
            LanguageVersion? languageVersion,
            CSharpCompilation compilation,
            GeneratorDriver generateResult,
            CancellationToken cancellationToken)
        {
            var runResult = generateResult.GetRunResult();
            if (runResult.GeneratedTrees.Length == 0
                && runResult.Results.Length == 0)
            {
                return;
            }

            var parseOptions = GetParseOptions(languageVersion);

            var diagnostics = await Task.WhenAll(runResult
                .GeneratedTrees
                .Select(async x => new
                {
                    FileName = Path.GetFileName(x.FilePath),
                    Diagnostics = await GetErrorDiagnostics(x, compilation, parseOptions, cancellationToken)
                }));

            var issues = diagnostics
                .Where(x => x.Diagnostics.Count > 0)
                .ToArray();

            Assert.Empty(issues);
        }

        private static async Task<IReadOnlyCollection<DiagnosticShort>> GetErrorDiagnostics(
            SyntaxTree syntaxTree,
            Compilation compilation,
            CSharpParseOptions parseOptions,
            CancellationToken ct)
        {
            var text = await syntaxTree.GetTextAsync(ct);

            return compilation
                .AddSyntaxTrees(
                    CSharpSyntaxTree.ParseText(text, parseOptions, cancellationToken: ct))
                .GetDiagnostics()
                .Where(d => d.Severity >= DiagnosticSeverity.Error
                    && !GetIgnoredErrors().Contains(d.Id))
                .Select(x => new DiagnosticShort(x, text))
                .ToArray();
        }

        private static SettingsTask VerifyGeneratedCode(
            GeneratorDriver generateResult,
            string callerFileName,
            string callerName,
            string? paramsNames)
        {
            return Configure(
                Verifier.Verify(generateResult),
                callerFileName,
                callerName,
                paramsNames);
        }

        private static SettingsTask Configure(
            SettingsTask settings,
            string callerFileName,
            string callerName,
            string? paramsNames)
        {
            settings = settings.DisableRequireUniquePrefix();

            if (!string.IsNullOrWhiteSpace(callerFileName))
            {
                var fileName = $"{Path.GetFileNameWithoutExtension(callerFileName)}.{callerName}";
                if (!string.IsNullOrWhiteSpace(paramsNames))
                {
                    fileName = $"{fileName}_{paramsNames}";
                }

                fileName = RemoveInvalidFileChars(fileName);

                settings = settings
                    .UseDirectory(Path.GetDirectoryName(callerFileName))
                    .UseFileName(fileName);
            }

            if (!string.IsNullOrWhiteSpace(paramsNames))
            {
                settings = settings
                    .UseTextForParameters(paramsNames);
            }

            return settings;
        }

        private static string RemoveInvalidFileChars(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();

            fileName = new string(fileName
                .Replace("<", "[")
                .Replace(">", "]")
                .Where(x => !invalidChars.Contains(x))
                .ToArray());
            return fileName;
        }

        private static AnalyzerConfigOptions CreateAnalyzerOptions(
            IDictionary<string, string>? buildPropertiesDictionary,
            string? generatedCodeNamespace,
            bool runInsideAzureFunctionProject)
        {
            var options = new Dictionary<string, string>()
            {
                ["is_global"] = true.ToString(),
                ["build_property.FunctionsEnableExecutorSourceGen"] = true.ToString(),
                ["build_property.FunctionsEnableMetadataSourceGen"] = true.ToString(),
                ["build_property.FunctionsGeneratedCodeNamespace"] = generatedCodeNamespace ?? "TestProject"
            };

            if (runInsideAzureFunctionProject)
            {
                options.Add("build_property.FunctionsExecutionModel", "isolated");
            }

            if (buildPropertiesDictionary is not null)
            {
                foreach (var pair in buildPropertiesDictionary)
                {
                    options[pair.Key] = pair.Value;
                }
            }

            var config = new AnalyzerConfigOptions(options);
            return config;
        }

        private static CSharpCompilation CreateCompilation(
            string inputSource,
            IEnumerable<Assembly>? extensionAssemblyReferences,
            LanguageVersion? languageVersion)
        {
            var syntaxTree = CreateSyntaxTree(inputSource, languageVersion);

            var metadata = GetAllAssemblies((extensionAssemblyReferences ?? Array.Empty<Assembly>())
                .Concat(new[]
                {
                    typeof(WorkerExtensionStartupAttribute).Assembly,
                    typeof(HttpTriggerAttribute).Assembly,
                    typeof(FunctionAttribute).Assembly,
                    typeof(HostBuilder).Assembly
                }))
                .Distinct()
                .Select(l => MetadataReference.CreateFromFile(l))
                .ToArray();

            var compilation = CSharpCompilation.Create(
                "TestProject",
                new[] { syntaxTree },
                metadata);

            return compilation;
        }

        private static SyntaxTree CreateSyntaxTree(
            string inputSource,
            LanguageVersion? languageVersion)
        {
            return CSharpSyntaxTree.ParseText(
                inputSource,
                GetParseOptions(languageVersion));
        }

        private static CSharpParseOptions GetParseOptions(LanguageVersion? languageVersion)
        {
            return new CSharpParseOptions(languageVersion ?? _defaultLanguageVersion);
        }

        private static bool AssertDiagnostics(CSharpCompilation compilation)
        {
            var errors = compilation.GetDiagnostics()
                .Where(d => d.Severity >= DiagnosticSeverity.Error
                    && !GetIgnoredErrors().Contains(d.Id))
                .ToArray();

            Assert.Empty(errors);
            return true;
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

        private static IEnumerable<string> GetIgnoredErrors()
        {
            yield return "CS5001"; // Program does not contain a static 'Main' method suitable for an entry point
        }

        private class DiagnosticShort
        {
            private const int TextRangeInfoChars = 30;

            public DiagnosticShort(
                Diagnostic diagnostic,
                SourceText text)
            {
                Id = diagnostic.Id;
                Severity = diagnostic.Severity;
                Message = diagnostic.GetMessage();
                IssueSyntaxLine = GetLine(diagnostic, text);
            }

            private static string? GetLine(
                Diagnostic diagnostic,
                SourceText text)
            {
                if (diagnostic.Location == Location.None)
                {
                    return null;
                }

                var span = diagnostic.Location.SourceSpan;

                var subText = text.GetSubText(
                    TextSpan.FromBounds(
                        Math.Max(0, span.Start - TextRangeInfoChars),
                        Math.Min(text.Length - 1, span.End + TextRangeInfoChars)));

                return subText.ToString();
            }

            public string Id { get; }
            public DiagnosticSeverity Severity { get; }
            public string Message { get; }
            public string? IssueSyntaxLine { get; }
        }
    }
}
