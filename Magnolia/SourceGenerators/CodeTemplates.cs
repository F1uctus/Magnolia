namespace Magnolia.SourceGenerators;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using static Extensions.StringExtensions;
using static Extensions.SymbolExtensions;

internal static class CodeTemplates {
    public static string Attribute(
        string              @namespace,
        string              name,
        AttributeTargets    targets,
        IEnumerable<string> properties
    ) {
        string targetList = string.Join(
            " | ",
            Enum.GetValues(typeof(AttributeTargets))
                .Cast<AttributeTargets>()
                .Where(a => targets.HasFlag(a))
                .Select(a => $"{nameof(AttributeTargets)}.{a}")
        );
        return $@"
using System;

namespace {@namespace}
{{
    [AttributeUsage({targetList}, Inherited = true, AllowMultiple = false)]
    public sealed class {name} : Attribute
    {{
{string.Join("", properties.Select(p => p + NL)).Indent(8)}
        public {name}()
        {{
        }}
    }}
}}".TrimStart();
    }

    public static string Class(
        INamedTypeSymbol    @class,
        IEnumerable<string> withReferencedNamespaces,
        IEnumerable<string> withProperties,
        IEnumerable<string> withMethods,
        bool                nullable
    ) {
        var @namespace = @class.ContainingNamespace.ToDisplayString();

        return (nullable ? "#nullable enable" : "")
             + $@"
{string.Join(NL, withReferencedNamespaces)}

namespace {@namespace}
{{
    {@class.GetAccessModifier()} partial class {@class.Name}
    {{
{string.Join(NL, withProperties).Indent(8)}

{string.Join(NL, withMethods).Indent(8)}
    }}
}}";
    }

    public static string VisitorMethod(IEnumerable<string> statements) {
        return $@"
public override void Traverse<T>(Action<T> visitor)
{{
{string.Join(";" + NL, statements).Indent(4)};
    if (this is T t)
    {{
        visitor(t);
    }}
}}".TrimStart();
    }

    public static string BindingPropertyForField(
        IFieldSymbol        field,
        string              getTemplate,
        string              setTemplate,
        IEnumerable<string> modifiers
    ) {
        string propertyName = FieldNameToPropertyName(field.Name);
        string refFieldName = NormalizeFieldNameInProperty(field.Name);
        var type = RemoveNullableAnnotation(
            field,
            "NodeList"
        );
        var attributes = field.GetAttributes().Select(a => $"[{a}]").ToArray();
        var mods = field.CustomModifiers
            .Select(m => m + " ")
            .Union(modifiers.Select(m => m + " "));
        return $@"
{string.Join(NL, attributes)}
public {string.Join("", mods)}{type} {propertyName}
{{
    get
{getTemplate.NamedFormat(
    ("field", refFieldName),
    ("prop", propertyName),
    ("type", field.Type.ToCode()),
    ("type!", field.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToCode())
).Indent(4)}
    set
{setTemplate.NamedFormat(
    ("field", refFieldName),
    ("prop", propertyName),
    ("type", field.Type.ToCode()),
    ("type!", field.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToCode())
).Indent(4)}
}}".TrimStart();
    }

    public static string WithPropertyParamsClassMethod(
        ITypeSymbol  @class,
        IFieldSymbol field
    ) {
        string propertyName = FieldNameToPropertyName(field.Name);
        var type = RemoveNullableAnnotation(
            field,
            "NodeList"
        );
        var listTypeArg = (field.Type as INamedTypeSymbol)?
            .TypeArguments.First()
            .Name;
        return $@"
public {@class.Name} With{propertyName}(params {listTypeArg}[] values)
{{
    {propertyName} = new {type}(this, (IEnumerable<{listTypeArg}>) values);
    return this;
}}".TrimStart();
    }

    public static string WithPropertyIEnumerableClassMethod(
        ITypeSymbol  @class,
        IFieldSymbol field
    ) {
        string propertyName = FieldNameToPropertyName(field.Name);
        var type = RemoveNullableAnnotation(
            field,
            "NodeList"
        );
        var listTypeArg = (field.Type as INamedTypeSymbol)?
            .TypeArguments.First()
            .Name;
        return $@"
public {@class.Name} With{propertyName}(IEnumerable<{listTypeArg}> values)
{{
    {propertyName} = new {type}(this, values);
    return this;
}}".TrimStart();
    }
}
