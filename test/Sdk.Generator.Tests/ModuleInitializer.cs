using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using VerifyTests;

namespace Microsoft.Azure.Functions.SdkGeneratorTests
{
    public static class ModuleInitializer
    {
        [ModuleInitializer]
        public static void Init()
        {
            VerifySourceGenerators.Initialize();
            VerifierSettings.RegisterFileConverter<GeneratedSourceResult>(Convert);
        }

        private static ConversionResult Convert(
            GeneratedSourceResult source,
            IReadOnlyDictionary<string, object> context)
        {
            var target = SourceToTarget(source);
            return new(null, new[] { target });
        }

        private static Target SourceToTarget(GeneratedSourceResult source)
        {
            var hintName = source.HintName;
            var data = $"""
                    //HintName: {hintName}
                    {source.SourceText}
                    """;
            var filePath = source.SyntaxTree.FilePath;
            if (filePath.Length > 0)
            {
                var extension = Path.GetExtension(filePath)[1..];
                var name = Path.GetFileNameWithoutExtension(filePath);
                return new(extension, data, name);
            }
            else
            {
                var name = Path.GetFileNameWithoutExtension(hintName);
                if (hintName.EndsWith(".vb"))
                {
                    return new("vb", data, name);
                }

                return new("cs", data, name);
            }
        }
    }
}
