using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace EventPipe;

/// <summary>
/// .NET symbols are super verbose and hard to read. This class is responsible to make them look like as close
/// as possible as their original source code.
/// </summary>
internal static class SymbolCleaner
{
    private static readonly SearchValues<char> TypeEndSearchValues = SearchValues.Create(['`', ',', ']', '>', '+', ')']);

    public static string Clean(string ns, string name, string signature)
    {
        StringBuilder sb = new(capacity: ns.Length + name.Length + signature.Length);
        AppendCleanedType(sb, ns);

        if (name == ".ctor")
        {
            sb.Insert(0, "new ");
        }
        else
        {
            sb.Append('.');
            sb.Append(name);
        }

        AppendCleanedArguments(sb, signature);
        return sb.ToString();
    }

    private static ReadOnlySpan<char> AppendCleanedType(StringBuilder sb, ReadOnlySpan<char> str)
    {
        bool subType = false;
        while (true)
        {
            str = TrimWordStart(str, "required_modifier ");
            str = TrimWordStart(str, "System.Runtime.InteropServices.InAttribute ");
            str = TrimWordStart(str, "value ");
            str = TrimWordStart(str, "class ");

            int typeNameEndIndex = str.IndexOfAny(TypeEndSearchValues);

            // Weird case where a subclass can contain angle brackets.
            while (typeNameEndIndex != -1
                   && str[typeNameEndIndex] == '>'
                   && subType)
            {
                int typeNameEndIndex2 = str[(typeNameEndIndex + 1)..].IndexOfAny(TypeEndSearchValues);
                typeNameEndIndex = typeNameEndIndex2 == -1 ? -1 : typeNameEndIndex + 1 + typeNameEndIndex2;
            }

            bool isWeirdTypeName = typeNameEndIndex != -1
                                   && str[typeNameEndIndex] == '>'
                                   && subType;
            if (isWeirdTypeName)
            {
                int typeNameEndIndex2 = str[(typeNameEndIndex + 1)..].IndexOfAny(TypeEndSearchValues);
                typeNameEndIndex = typeNameEndIndex2 == -1 ? -1 : typeNameEndIndex + 1 + typeNameEndIndex2;
            }

            // A ']' was found, continue the search if it's from an array type '[]'.
            if (typeNameEndIndex != -1
                && str[typeNameEndIndex] == ']'
                && str[typeNameEndIndex - 1] == '[')
            {
                int typeNameEndIndex2 = str[(typeNameEndIndex + 1)..].IndexOfAny(TypeEndSearchValues);
                typeNameEndIndex = typeNameEndIndex2 == -1 ? -1 : typeNameEndIndex + 1 + typeNameEndIndex2;
            }

            if (typeNameEndIndex == -1
                || str[typeNameEndIndex] is ',' or ']' or ')' or '>')
            {
                var typeName = typeNameEndIndex == -1 ? str : str[..typeNameEndIndex];
                AppendPrettifiedType(sb, typeName);
                return str[typeName.Length..];
            }

            if (str[typeNameEndIndex] == '`')
            {
                AppendPrettifiedType(sb, str[..typeNameEndIndex]);
                str = AppendTypeArguments(sb, str[typeNameEndIndex..]);
            }
            else if (str[typeNameEndIndex] == '+')
            {
                AppendPrettifiedType(sb, str[..typeNameEndIndex]);
                sb.Append('+');
                str = str[(typeNameEndIndex + 1)..];
                subType = true;
            }

            if (str.Length == 0)
            {
                return str;
            }
        }
    }

    private static ReadOnlySpan<char> AppendTypeArguments(StringBuilder sb, ReadOnlySpan<char> ns)
    {
        Debug.Assert(ns[0] == '`');

        int typeArgumentsCount = CharToInt(ns[1]);

        sb.Append('<');
        if (ns.Length > 2 && ns[2] is '[' or '<') // The type arguments are present.
        {
            ns = ns[3..]; // Move inside the brackets.
            while (ns[0] is not ']' and not '>')
            {
                ns = AppendCleanedType(sb, ns);
                sb.Append(", ");

                if (ns[0] == ',')
                {
                    ns = ns[1..];
                }
            }

            ns = ns[1..];
        }
        else
        {
            for (int i = 0; i < typeArgumentsCount; i += 1)
            {
                sb.Append("T, ");
            }

            ns = ns[2..];
        }

        sb.Length -= ", ".Length;
        sb.Append('>');

        return ns;
    }

    private static void AppendCleanedArguments(StringBuilder sb, ReadOnlySpan<char> signature)
    {
        int openingParenthesisIndex = signature.IndexOf('(');
        if (openingParenthesisIndex == -1)
        {
            return;
        }

        sb.Append('(');

        signature = signature[(openingParenthesisIndex + 1)..]; // Move inside the parenthesis.

        if (signature[0] != ')')
        {
            while (signature.Length > 0)
            {
                signature = AppendCleanedType(sb, signature);
                sb.Append(", ");
                signature = signature[1..];
            }

            sb.Length -= ", ".Length;
        }

        sb.Append(')');
    }

    private static void AppendPrettifiedType(StringBuilder sb, ReadOnlySpan<char> ns)
    {
        if (ns.Length == 0)
        {
            return;
        }

        string suffix = "";
        if (ns[^1] == '&')
        {
            suffix = "&";
            ns = ns[..^1];
        }
        else if (ns[^1] == '*')
        {
            suffix = "*";
            ns = ns[..^1];
        }

        if (ns.Equals("System.__Canon", StringComparison.Ordinal)
            || (ns.Length >= 2 && ns[0] == '!'))
        {
            sb.Append('T');
            sb.Append(suffix);
            return;
        }

        sb.Append(ns);
        sb.Append(suffix);
    }

    private static ReadOnlySpan<char> TrimWordStart(ReadOnlySpan<char> span, ReadOnlySpan<char> word)
    {
        return span.StartsWith(word) ? span[word.Length..] : span;
    }

    private static int CharToInt(char c)
    {
        return c - '0';
    }
}