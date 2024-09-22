using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.PrecompiledFunctionMetadataProviderGenerator
{
    [DebuggerDisplay("{ToString()}")]
    internal sealed record ParsedType
    {
        private const string TaskClass = "System.Threading.Tasks.Task";
        private const string ValueTaskClass = "System.Threading.Tasks.ValueTask";

        private ParsedType(
            string? type,
            string? asyncWrapper,
            bool isEnumerable,
            bool isAsyncEnumerable,
            bool isBinary)
        {
            var hasAsync = !string.IsNullOrWhiteSpace(asyncWrapper);
            var hasType = !string.IsNullOrWhiteSpace(type);
            if (!hasAsync && !hasType)
            {
                throw new ArgumentException("At least 1 argument must be provided");
            }

            if (hasAsync && hasType)
            {
                RawType = type!;
                FullType = $"{asyncWrapper}<{type}>";
                IsAwaitable = true;
            }

            if (hasAsync && !hasType)
            {
                RawType = asyncWrapper!;
                FullType = asyncWrapper!;
                IsAwaitable = true;
            }

            if (!hasAsync && hasType)
            {
                RawType = type!;
                FullType = type!;
                IsAwaitable = false;
            }

            IsEnumerable = isEnumerable;
            IsAsyncEnumerable = isAsyncEnumerable;
        }

        public string RawType { get; } = default!;
        public string FullType { get; } = default!;
        public bool IsAwaitable { get; }
        public bool IsEnumerable { get; }
        public bool IsAsyncEnumerable { get; }

        public override string ToString()
            => FullType;

        public static bool TryParse(
            ITypeSymbol symbol,
            out ParsedType? type,
            out ITypeSymbol rawOutputSymbol,
            out Diagnostic? diagnostic)
        {
            rawOutputSymbol = symbol;
            type = null;
            diagnostic = null;

            var fullName = symbol.ToString();
            type = fullName switch
            {
                TaskClass => new ParsedType(null, TaskClass, false, false, false),
                ValueTaskClass => new ParsedType(null, ValueTaskClass, false, false),
                "void" => new ParsedType("void", null, false, false),
                "string" => new ParsedType("string", null, false, false),
                _ => null
            };

            if (type is not null)
            {
                return true;
            }

            if (symbol is not INamedTypeSymbol namedSymbol)
            {
                diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.SymbolNotFound,
                    symbol.Locations.FirstOrDefault() ?? Location.None,
                    new[] { "INamedTypeSymbol" });
                return false;
            }

            type = TryParse(fullName, TaskClass, namedSymbol, out rawOutputSymbol)
                ?? TryParse(fullName, ValueTaskClass, namedSymbol, out rawOutputSymbol);
            if (type is not null)
            {
                if (type.IsAsyncEnumerable)
                {
                    diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.InvalidBindingType,
                        symbol.Locations.FirstOrDefault() ?? Location.None,
                        new[] { fullName, "return" });
                    return false;
                }

                return true;
            }

            var isEnumerable = HasEnumerableInterface(symbol);
            var isAsyncEnumerable = HasAsyncEnumerableInterface(symbol);
            type = new ParsedType(fullName, null, isEnumerable, isAsyncEnumerable);
            return type is not null;
        }

        public bool IsString =>

        private static ParsedType? TryParse(
            string parsedType,
            string type,
            INamedTypeSymbol symbol,
            out ITypeSymbol innerTypeSymbol)
        {
            innerTypeSymbol = null;
            if (parsedType.Length <= type.Length + 2
                || parsedType[type.Length] != '<'
                || parsedType[parsedType.Length - 1] != '>')
            {
                return null;
            }

            for (int i = 0; i < type.Length; i++)
            {
                if (parsedType[i] != type[i])
                {
                    return null;
                }
            }

            var innerType = parsedType.Substring(type.Length + 1, parsedType.Length - type.Length - 2);
            innerTypeSymbol = symbol.TypeArguments.Single();
            var isEnumerable = HasEnumerableInterface(innerTypeSymbol);
            var isAsyncEnumerable = HasAsyncEnumerableInterface(innerTypeSymbol);
            return new ParsedType(innerType, type, isEnumerable, isAsyncEnumerable);
        }

        private static bool HasAsyncEnumerableInterface(ITypeSymbol type)
        {
            const string ExpectedName = "IAsyncEnumerable";
            const string ExpectedNamespace = "System.Collections.Generic";

            return type is not null
                && ((type.Name == ExpectedName && type.ContainingNamespace.ToString() == ExpectedNamespace)
                    || type.AllInterfaces.Any(x => x.IsGenericType && x.Name == ExpectedName && x.ContainingNamespace.ToString() == ExpectedNamespace));
        }

        private static bool HasEnumerableInterface(ITypeSymbol symbol)
        {
            return symbol is not null
                && symbol.AllInterfaces.Any(x => x.Name == "IEnumerable"
                    && x.ToString() == "System.Collections.IEnumerable");
        }
    }
}
