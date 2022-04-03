namespace Magnolia.SourceGenerators.Extensions;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class SymbolExtensions {
    internal static string GetAccessModifier(this ISymbol classSymbol) {
        return classSymbol.DeclaredAccessibility.ToString().ToLowerInvariant();
    }

    internal static string ToCode(this ISymbol symbol) {
        return symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
    }

    internal static IEnumerable<IFieldSymbol> GetBoundFields(
        this ITypeSymbol classSymbol
    ) {
        var targetSymbolMembers = classSymbol
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(x => x.CanBeReferencedByName);
        return targetSymbolMembers;
    }

    internal static bool DefaultEquals(this ISymbol symbol, ISymbol other) {
        return SymbolEqualityComparer.Default.Equals(symbol, other);
    }

    public static T? GetNamedArg<T>(
        this AttributeData attr,
        string             name
    ) {
        var arg = attr.NamedArguments.FirstOrDefault(a => a.Key == name).Value.Value;
        if (arg is T t) {
            return t;
        }

        return default;
    }

    public static AttributeData GetAttribute(
        this ISymbol     symbol,
        INamedTypeSymbol attributeType
    ) {
        return symbol.GetAttributes()
            .First(a => a.AttributeClass!.DefaultEquals(attributeType));
    }

    internal static bool TryGetAttributes(
        this ISymbol                   symbol,
        INamedTypeSymbol               attributeType,
        out IEnumerable<AttributeData> attributes
    ) {
        attributes = symbol.GetAttributes()
            .Where(a => a.AttributeClass!.DefaultEquals(attributeType));
        return attributes.Any();
    }

    internal static bool HasAttribute(
        this ISymbol     symbol,
        INamedTypeSymbol attributeType
    ) {
        return symbol.GetAttributes()
            .Any(a => a.AttributeClass!.DefaultEquals(attributeType));
    }

    internal static IEnumerable<(ClassDeclarationSyntax, INamedTypeSymbol)>
        GetClassSymbols(
            this Compilation compilation,
            SyntaxReceiver   receiver
        ) {
        return receiver.CandidateClasses.Select(
            c =>
                (c, GetClassSymbol(compilation, c))
        );
    }

    internal static INamedTypeSymbol GetClassSymbol(
        this Compilation compilation,
        SyntaxNode       clazz
    ) {
        var model = compilation.GetSemanticModel(clazz.SyntaxTree);
        var classSymbol = model.GetDeclaredSymbol(clazz)!;
        return (INamedTypeSymbol) classSymbol;
    }

    internal static string RemoveNullableAnnotation(
        IFieldSymbol    forField,
        params string[] withContainerTypeNames
    ) {
        return forField.Type.WithNullableAnnotation(
                withContainerTypeNames.Any(ctn => forField.Type.Name.StartsWith(ctn))
                    ? NullableAnnotation.NotAnnotated
                    : forField.Type.NullableAnnotation
            )
            .ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
    }
}
