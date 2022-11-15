using System.Text;
using System.Text.RegularExpressions;

namespace CodeVideoMaker.Highlighting;

class JavaScriptHighlighter : Highlighter
{
    private static string[] keywords = new[]
    {
        "async",
        "await",
        "break",
        "case",
        "catch",
        "class",
        "constructor",
        "continue",
        "const",
        "debugger",
        "default",
        "delete",
        "do",
        "else",
        "export",
        "extends",
        "false",
        "finally",
        "for",
        "function",
        "goto",
        "if",
        "import",
        "in",
        "instanceof",
        "let",
        "new",
        "null",
        "return",
        "super",
        "switch",
        "this",
        "throw",
        "true",
        "try",
        "typeof",
        "var",
        "while",
        "with",
        "yield"
    };

    private static readonly Regex regex = new Regex(@"
        (?<comment>//.*?$)
        |(?<string1>"".*?(""|$))
        |(?<string2>'.*?('|$))
        |(?<string3>`.*?(`|$))
        |(?<number1>\b\d+\b)
        |(?<number2>\b0x[0-9a-fA-F]+\b)
        |(?<number3>\b\d+\.\d+\b)
        |(?<number4>\b\d+\.\d+[eE][+-]?\d+\b)
        |(?<number5>\b\d+[eE][+-]?\d+\b)
        |(?<operator>\+|\-|\*|/|%|&|\||\^|~|!|<|>|=|\?|:|\.|,|\(|\)|\[|\]|\{|\})
        |(?<reserved>\b[A-Z][a-zA-Z0-9_]+\b)
        |(?<framework>\b(window|document|console)\b)
        |(?<identifier>\b[a-z_][a-zA-Z0-9_]*\b)
    ", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

    public override string Highlight(string input)
    {
        var output = new StringBuilder();
        var matches = regex.Matches(input);
        int i = 0;
        foreach (Match match in matches)
        {
            if (match.Index > i)
            {
                output.Append(new string(' ', match.Index - i));
            }
            i = match.Index;

            var groupName = GetGroupName(match) ?? " ";
            output.Append(new string(groupName[0], match.Length));
            i += match.Length;
        }

        if (i < input.Length)
        {
            output.Append(new string(' ', input.Length - i));
        }

        return output.ToString();
    }

    private static string? GetGroupName(Match match)
    {
        foreach (Group group in match.Groups)
        {
            if (int.TryParse(group.Name, out _))
            {
                continue;
            }
            if (group.Success)
            {
                if (group.Name == "identifier")
                {
                    if (Array.IndexOf(keywords, group.Value) >= 0)
                    {
                        return "keyword";
                    }
                }
                return group.Name;
            }
        }
        return null;
    }
}