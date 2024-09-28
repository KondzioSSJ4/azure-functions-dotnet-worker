using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using VerifyTests;
using VerifyXunit;
using Xunit;

namespace Microsoft.Azure.Functions.SdkGeneratorTests.Helpers
{
    internal sealed class SourceGeneratorResult
    {
        private readonly CSharpCompilation _compilation;
        private readonly GeneratorDriver _driver;
        private readonly CSharpParseOptions _parseOptions;
        private readonly GeneratorDriverRunResult _generatorDriverRunResult;
        private readonly IReadOnlyCollection<string> _ignoredErrors;
        private readonly List<Task> _validationTasks = new();

        private string? _temporalLocation;

        public SourceGeneratorResult(
            CSharpCompilation compilation,
            GeneratorDriver driver,
            CSharpParseOptions parseOptions,
            GeneratorDriverRunResult generatorDriverRunResult,
            IReadOnlyCollection<string> ignoredErrors)
        {
            _compilation = compilation;
            _driver = driver;
            _parseOptions = parseOptions;
            _generatorDriverRunResult = LoadDiagnostics(generatorDriverRunResult);
            _ignoredErrors = ignoredErrors;
        }

        private static GeneratorDriverRunResult LoadDiagnostics(GeneratorDriverRunResult result)
        {
            _ = result.Diagnostics;
            _ = result.GeneratedTrees;
            return result;
        }

        internal string GetTemporalAssemblyFile()
        {
            if (!string.IsNullOrWhiteSpace(_temporalLocation))
            {
                return _temporalLocation;
            }

            AssertNoErrors();

            var compilation = _compilation.AddSyntaxTrees(_generatorDriverRunResult.GeneratedTrees);

            var path = Path.GetTempFileName();
            var emited = compilation.Emit(path);
            Assert.True(emited.Success);

            _temporalLocation = path;
            return path;
        }

        public TaskAwaiter GetAwaiter()
        {
            return ToTask().GetAwaiter();
        }

        private async Task ToTask()
        {
            foreach (var item in _validationTasks)
            {
                await item;
            }
        }

        private void AssertNoErrors()
        {
            Assert.Empty(_generatorDriverRunResult
                .Diagnostics
                .Where(x => x.Severity >= DiagnosticSeverity.Error));
        }

        internal SourceGeneratorResult VerifyOutput(
            string? paramsNames = null,
            [CallerFilePath] string callerFileName = "",
            [CallerMemberName] string callerName = "")
        {
            _validationTasks.Add(
                Configure(
                    Verifier.Verify(_generatorDriverRunResult),
                    callerFileName,
                    callerName,
                    paramsNames));

            return this;
        }

        internal SourceGeneratorResult VerifySpecifiedFile(
            string fileName,
            string parameters = "",
            [CallerFilePath] string callerFileName = "",
            [CallerMemberName] string callerName = "")
        {
            _validationTasks.Add(
                Configure(
                    Verifier.Verify(_generatorDriverRunResult)
                        .IgnoreGeneratedResult(x => x.HintName != fileName),
                    callerFileName,
                    callerName,
                    parameters));

            return this;
        }

        internal SourceGeneratorResult VerifyDiagnosticsOnly(
            string parameters = "",
            [CallerFilePath] string callerFileName = "",
            [CallerMemberName] string callerName = "")
        {
            _validationTasks.Add(
               Configure(
                   Verifier.Verify(_generatorDriverRunResult)
                        .IgnoreGeneratedResult(x => true),
                    callerFileName,
                    callerName,
                    parameters));

            return this;
        }

        internal SourceGeneratorResult VerifyOutput<T>(
            Func<GeneratorDriverRunResult, T> getValueToVerify,
            string? paramsNames = null,
            [CallerFilePath] string callerFileName = "",
            [CallerMemberName] string callerName = "")
        {
            _validationTasks.Add(
                Configure(
                    Verifier.Verify(getValueToVerify.Invoke(_generatorDriverRunResult)),
                    callerFileName,
                    callerName,
                    paramsNames));

            return this;
        }

        internal SourceGeneratorResult ValidateGeneratorDiagnostics(
            Action<IReadOnlyCollection<Diagnostic>> assert)
        {
            _validationTasks.Add(Task.Run(() =>
            {
                assert.Invoke(_generatorDriverRunResult.Diagnostics);
            }));

            return this;
        }

        internal SourceGeneratorResult AssertDiagnosticsOfGeneratedCode(
            CancellationToken cancellationToken = default)
        {
            _validationTasks.Add(
                Task.Run(async () =>
                {
                    var runResult = _generatorDriverRunResult;
                    if (runResult.GeneratedTrees.Length == 0
                        && runResult.Results.Length == 0)
                    {
                        return;
                    }

                    var diagnostics = await Task.WhenAll(runResult
                        .GeneratedTrees
                        .Select(async x => new
                        {
                            FileName = Path.GetFileName(x.FilePath),
                            Diagnostics = await GetErrorDiagnostics(x, cancellationToken)
                        }));

                    var issues = diagnostics
                        .Where(x => x.Diagnostics.Count > 0)
                        .ToArray();

                    Assert.Empty(issues);
                }));

            return this;
        }

        private async Task<IReadOnlyCollection<DiagnosticShort>> GetErrorDiagnostics(
            SyntaxTree syntaxTree,
            CancellationToken ct)
        {
            var text = await syntaxTree.GetTextAsync(ct);

            return _compilation
                .AddSyntaxTrees(
                    CSharpSyntaxTree.ParseText(text, _parseOptions, cancellationToken: ct))
                .GetDiagnostics()
                .Where(d => d.Severity >= DiagnosticSeverity.Error
                    && !_ignoredErrors.Contains(d.Id))
                .Select(x => new DiagnosticShort(x, text))
                .ToArray();
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

        [DebuggerDisplay("[{Severity}] {Id} '{Message}' in '{IssueSyntaxLine}'")]
        private class DiagnosticShort
        {
            private const int TextRangeInfoChars = 10;

            public DiagnosticShort(
                Diagnostic diagnostic,
                SourceText text)
            {
                Id = diagnostic.Id.Trim('"');
                Severity = diagnostic.Severity;
                Message = diagnostic.GetMessage().Trim('"');
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

                return subText.ToString().Trim();
            }

            public string Id { get; }
            public DiagnosticSeverity Severity { get; }
            public string Message { get; }
            public string? IssueSyntaxLine { get; }
        }
    }
}
