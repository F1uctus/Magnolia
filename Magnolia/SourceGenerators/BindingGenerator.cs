namespace Magnolia.SourceGenerators;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Attributes;
using Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Extensions.StringExtensions;
using static CodeTemplates;
using static Microsoft.CodeAnalysis.NullableAnnotation;

[Generator]
internal class BindingGenerator : ISourceGenerator {
    readonly GeneratedAttribute branchAttr = new(
        "Branch",
        AttributeTargets.Class | AttributeTargets.Struct
    );

    readonly GeneratedAttribute leafAttr = new(
        "Leaf",
        AttributeTargets.Property | AttributeTargets.Field,
        "public bool Virtual { get; set; }"
    );

    public void Initialize(GeneratorInitializationContext context) {
        // if (!Debugger.IsAttached) {
        //     Debugger.Launch();
        // }

        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context) {
        context.AddSource(branchAttr.Name, branchAttr.Code);
        context.AddSource(leafAttr.Name, leafAttr.Code);

        if (context.SyntaxReceiver is not SyntaxReceiver receiver) {
            return;
        }

        var compilation = BuildCompilation(context);
        branchAttr.BindTo(compilation);
        leafAttr.BindTo(compilation);

        foreach (var (cds, symbol) in compilation
                     .GetClassSymbols(receiver)
                     .Where(x => x.Item2.HasAttribute(branchAttr.Symbol))) {
            context.AddSource(
                $"{symbol.Name}.g.cs",
                SourceText.From(GenerateClassCode(cds, symbol), Encoding.UTF8)
            );
        }
    }

    Compilation BuildCompilation(GeneratorExecutionContext context) {
        var options = context.Compilation.SyntaxTrees.First().Options
            as CSharpParseOptions;

        return context.Compilation
            .AddSyntaxTrees(
                CSharpSyntaxTree.ParseText(
                    branchAttr.Code,
                    options
                )
            )
            .AddSyntaxTrees(
                CSharpSyntaxTree.ParseText(
                    leafAttr.Code,
                    options
                )
            );
    }

    string GenerateClassCode(
        ClassDeclarationSyntax declaration,
        INamedTypeSymbol       @class
    ) {
        var leaves = @class.GetBoundFields()
            .Where(f => f.HasAttribute(leafAttr.Symbol))
            .ToArray();
        var usings = new HashSet<string>(
            ((CompilationUnitSyntax)
                declaration.SyntaxTree.GetRoot()).Usings.Select(u => u.ToString())
        ) {
            Using("System"),
            Using("System.Collections.Generic"),
            Using("Magnolia.Trees.Paths")
        };
        return Class(
            @class,
            usings,
            leaves
                .Select(f => GeneratePropertyCode(@class, f))
                .ToArray(),
            new[] { GenerateVisitorCode(leaves) },
            true
        );
    }

    string GeneratePropertyCode(INamedTypeSymbol @class, IFieldSymbol field) {
        var modifiers = new HashSet<string>();
        if (field.GetAttribute(leafAttr.Symbol)
                .GetNamedArg<bool?>("Virtual") is true) {
            modifiers.Add("virtual");
        }

        if (field.Type.Name.StartsWith("NodeList")) {
            var typeArg1 = ((INamedTypeSymbol) field.Type).TypeArguments[0].ToCode();
            var typeArg2 = ((INamedTypeSymbol) field.Type).TypeArguments[1].ToCode();
            return
                BindingPropertyForField(
                    field,
                    string.Join(
                        NL,
                        "{",
                        "    if ({field} == null)",
                        "    {",
                        "        {field} = new {type!}(this);",
                        "    }",
                        "    return {field};",
                        "}"
                    ),
                    string.Join(
                        NL,
                        "{",
                        "    for (var i = 0; i < value.Count; i++)",
                        "    {",
                        "        if (value[i] is { } node)",
                        "        {",
                        "            node.Parent = this;",
                        $"            node.Path   = new NodeListTreePath<{typeArg1}, {typeArg2}>(value, i);",
                        "        }",
                        "    }",
                        "    {field} = value;",
                        "    AfterPropertyBinding(value);",
                        "}"
                    ),
                    modifiers
                )
              + NL2
              + WithPropertyParamsClassMethod(@class, field)
              + NL2
              + WithPropertyIEnumerableClassMethod(@class, field);
        }

        var treeNodeType = @class;
        while (treeNodeType?.Name != "TreeNode") {
            treeNodeType = treeNodeType!.BaseType;
        }

        var rootType = treeNodeType.TypeArguments[0].ToCode();

        return BindingPropertyForField(
            field,
            "=> {field};",
            field.Type.NullableAnnotation == Annotated
                ? string.Join(
                    NL,
                    "{",
                    "    if (value != null)",
                    "    {",
                    "        value.Parent = this;",
                    $"        value.Path = new NodeTreePath<{{type!}}, {rootType}>(",
                    "            value,",
                    $"            typeof({@class.ToCode()}).GetProperty(\"{{prop}}\")!",
                    "        );",
                    "    }",
                    "    {field} = value;",
                    "    AfterPropertyBinding(value);",
                    "}"
                )
                : string.Join(
                    NL,
                    "{",
                    "    value.Parent = this;",
                    $"    value.Path = new NodeTreePath<{{type!}}, {rootType}>(",
                    "        value,",
                    $"        typeof({@class.ToCode()}).GetProperty(\"{{prop}}\")!",
                    "    );",
                    "    {field} = value;",
                    "    AfterPropertyBinding(value);",
                    "}"
                ),
            modifiers
        );
    }

    static string GenerateVisitorCode(IEnumerable<IFieldSymbol> leaves) {
        return VisitorMethod(
            leaves
                .Where(
                    l => !l.GetAttributes()
                        .Any(a => a.AttributeClass?.Name == nameof(NotTraversableAttribute))
                )
                .Select(
                    l => {
                        var pName = FieldNameToPropertyName(l.Name);
                        return string.Format(
                            l.Type.NullableAnnotation == Annotated
                                ? $@"if ({pName} != null) {{0}}"
                                : "{0}",
                            pName + ".Traverse(visitor)"
                        );
                    }
                )
        );
    }
}
