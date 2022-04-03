namespace Magnolia.SourceGenerators;

using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

internal class GeneratedAttribute {
    public const string Namespace =
        nameof(Magnolia)
      + "."
      + nameof(Attributes);

    public GeneratedAttribute(
        string           name,
        AttributeTargets targets,
        params string[]  properties
    ) {
        Name = name + "Attribute";
        Code = SourceText.From(
            CodeTemplates.Attribute(Namespace, Name, targets, properties),
            Encoding.UTF8
        );
    }

    public string Name { get; }
    public SourceText Code { get; }
    public INamedTypeSymbol Symbol { get; private set; } = null!;

    public void BindTo(Compilation compilation) {
        Symbol = compilation.GetTypeByMetadataName(
            $"{Namespace}.{Name}"
        )!;
    }
}
