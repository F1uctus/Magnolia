namespace Magnolia.SourceGenerators.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

internal static class StringExtensions {
    public static readonly string NL = Environment.NewLine;
    public static readonly string NL2 = NL + NL;

    internal static string Indent(this string value, int size) {
        var strArray = value.Split('\n');
        var sb = new StringBuilder();
        for (var i = 0; i < strArray.Length - 1; i++) {
            strArray[i] = strArray[i].TrimEnd();
            if (string.IsNullOrWhiteSpace(strArray[i])) {
                sb.AppendLine();
            }
            else {
                sb.Append(new string(' ', size)).Append(strArray[i]).AppendLine();
            }
        }

        if (!string.IsNullOrWhiteSpace(strArray[strArray.Length - 1])) {
            sb.Append(new string(' ', size)).Append(strArray[strArray.Length - 1]);
        }

        return sb.ToString();
    }

    internal static string NamedFormat(
        this string                source,
        Dictionary<string, object> parameters
    ) {
        return parameters.Aggregate(
            source,
            (current, parameter) =>
                current.Replace(parameter.Key, parameter.Value.ToString())
        );
    }

    internal static string NamedFormat(
        this   string             source,
        params (string, object)[] parameters
    ) {
        return parameters.Aggregate(
            source,
            (current, parameter) =>
                current.Replace(
                    "{" + parameter.Item1 + "}",
                    parameter.Item2.ToString()
                )
        );
    }

    internal static string FieldNameToPropertyName(string fieldName) {
        return char.ToUpper(fieldName[0]) + fieldName.Substring(1);
    }

    internal static string NormalizeFieldNameInProperty(string fieldName) {
        return fieldName == "value"
            ? "this.value"
            : SyntaxFacts.GetKeywordKind(fieldName) == SyntaxKind.None
           && SyntaxFacts.GetContextualKeywordKind(fieldName) == SyntaxKind.None
                ? fieldName
                : $"@{fieldName}";
    }

    internal static string Using(string @namespace) {
        return $"using {@namespace};";
    }
}
