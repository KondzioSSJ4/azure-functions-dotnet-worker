using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Azure.Functions.Worker.Sdk.Generators.Extensions
{
    internal static class SyntaxExtensions
    {
        public static TypedConstant? GetArgumentByConstructor(
            this AttributeData attribute,
            int constructorArgumentIndex)
        {
            if (attribute is null
                || attribute.ConstructorArguments.Length <= constructorArgumentIndex)
            {
                return null;
            }

            return attribute.ConstructorArguments[constructorArgumentIndex];
        }

        public static TypedConstant? GetArgumentByName(
            this AttributeData attribute,
            string argumentName)
        {
            if (attribute is null
                || string.IsNullOrWhiteSpace(argumentName))
            {
                return null;
            }

            var named = attribute.NamedArguments
                    .Where(x => string.Equals(x.Key, argumentName, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Value)
                    .FirstOrDefault();

            return named.IsNull
                ? null
                : named;
        }

        public static bool IsInheritedFrom(
            this INamedTypeSymbol symbol,
            string namespaceName,
            string className)
        {
            while (symbol?.ConstructedFrom is not null)
            {
                var from = symbol.ConstructedFrom;
                if (string.Equals(from.ContainingNamespace?.ToString() ?? string.Empty, namespaceName ?? string.Empty)
                    && string.Equals(from.Name, className))
                {
                    return true;
                }

                symbol = symbol.BaseType;
            }

            return false;
        }

        public static string GetFullName(this ClassDeclarationSyntax classSyntax)
        {
            return IncludeNamespace(classSyntax, classSyntax.Identifier.ValueText);
        }

        public static string GetFullName(this RecordDeclarationSyntax recordSyntax)
        {
            return IncludeNamespace(recordSyntax, recordSyntax.Identifier.ValueText);
        }

        private static string IncludeNamespace(SyntaxNode node, string objectName)
        {
            var wrapped = false;

            var parentNamespace = node.GetParentOfType<NamespaceDeclarationSyntax>();
            while (parentNamespace is not null)
            {
                wrapped = true;
                var namespaceText = parentNamespace.Name.ToString();
                if (!string.IsNullOrEmpty(namespaceText))
                {
                    objectName = string.Join(".", namespaceText, objectName);
                }

                parentNamespace = parentNamespace.GetParentOfType<NamespaceDeclarationSyntax>();
            }

            if (wrapped)
            {
                return objectName;
            }

            var fileNamespace = node
                .GetTopLevelParent()
                .GetChildOfType<FileScopedNamespaceDeclarationSyntax>()
                .FirstOrDefault();

            if (fileNamespace is not null)
            {
                return $"{fileNamespace.Name}.{objectName}";
            }

            return objectName;
        }

        public static T? GetParentOfType<T>(this SyntaxNode node)
            where T : SyntaxNode
        {
            if (node?.Parent is null)
            {
                return null;
            }

            var current = node.Parent;
            while (current is not null)
            {
                if (current is T expectedSyntax)
                {
                    return expectedSyntax;
                }

                current = current.Parent;
            }

            return null;
        }

        private static IEnumerable<T> GetChildOfType<T>(this SyntaxNode node)
            where T : SyntaxNode
        {
            if (node is null)
            {
                yield break;
            }

            foreach (var item in node.ChildNodes())
            {
                if (item is T casted)
                {
                    yield return casted;
                }
                else
                {
                    foreach (var nestedChild in item.GetChildOfType<T>())
                    {
                        yield return nestedChild;
                    }
                }
            }
        }

        private static SyntaxNode GetTopLevelParent(
            this SyntaxNode node)
        {
            while (node.Parent is not null)
            {
                node = node.Parent;
            }

            return node;
        }
    }
}
