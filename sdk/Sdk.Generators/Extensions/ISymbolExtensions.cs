// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using Microsoft.Azure.Functions.Worker.Sdk.Generators.Extensions;
using Microsoft.CodeAnalysis;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators
{
    internal static class ISymbolExtensions
    {
        /// <summary>
        /// Walks the symbol tree to generate the fully qualified name of a type symbol.
        /// Ex input: A Symbol for "Task" token
        /// Output: "System.Threading.Tasks.Task"
        /// </summary>
        internal static string GetFullName(this ITypeSymbol typeSymbol)
        {
            var symbol = typeSymbol as ISymbol;

            if (symbol == null || IsRootNamespace(symbol))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(symbol.MetadataName);

            if (symbol is IArrayTypeSymbol arraySymbol) // arrays need to be handled differently b/c the properties used to get the full name for other symbols are null for IArrayTypeSymbols
            {
                sb.Append(arraySymbol.ElementType.GetFullName()); // ex: for string[], the ElementType is System.String and that is the full name returned at this step.
                sb.Append("[]"); // System.Byte[], System.String[] are the full names for array types of element type Byte, String and we auto-add the brackets here.
            }
            else
            {
                symbol = symbol.ContainingSymbol;

                while (!IsRootNamespace(symbol))
                {
                    sb.Insert(0, '.');
                    sb.Insert(0, symbol.MetadataName);
                    symbol = symbol.ContainingSymbol;
                }
            }

            return sb.ToString();
        }

        private static bool IsRootNamespace(ISymbol symbol)
        {
            if (symbol is INamespaceSymbol namespaceSymbol)
            {
                return namespaceSymbol.IsGlobalNamespace;
            }

            return false;
        }

        internal static bool IsOrDerivedFrom(this ITypeSymbol symbol, ITypeSymbol? other)
        {
            if (other is null)
            {
                return false;
            }

            var current = symbol;

            while (current != null)
            {
                if (SymbolEqualityComparer.Default.Equals(current, other) || SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, other))
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        internal static bool IsOrImplements(this ITypeSymbol symbol, ITypeSymbol? other)
        {
            if (other is null)
            {
                return false;
            }

            var current = symbol;

            while (current != null)
            {
                foreach (var member in current.Interfaces)
                {
                    if (member.IsOrDerivedFrom(other))
                    {
                        return true;
                    }
                }

                current = current.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Used to know if attribute support retry
        /// Keep in mind that attributes may get retry support after time, so we should not be limited of current attributes state
        /// </summary>
        internal static bool IsRetrySupported(
            this AttributeData attribute)
        {
            if (attribute?.AttributeClass is null)
            {
                return false;
            }

            var bindingCapabilities = attribute
                .AttributeClass
                .GetAttributes()
                .Where(x => x.AttributeClass?.ContainingNamespace is not null
                    && x.AttributeClass.Name == "BindingCapabilitiesAttribute"
                    && x.AttributeClass.ContainingNamespace.ToString() == "Microsoft.Azure.Functions.Worker.Extensions.Abstractions");

            foreach (var item in bindingCapabilities)
            {
                var arguments = item.GetArgumentByConstructor(0);
                if (!arguments.HasValue
                    || arguments.Value.IsNull
                    || !arguments.Value.Values.Any(x => x.Value is not null
                        && x.Value.ToString() == Constants.BindingCapabilities.FunctionLevelRetry))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        internal static bool IsOrImplementsOrDerivesFrom(this ITypeSymbol symbol, ITypeSymbol? other)
        {
            return symbol.IsOrImplements(other) || symbol.IsOrDerivedFrom(other);
        }
    }
}
