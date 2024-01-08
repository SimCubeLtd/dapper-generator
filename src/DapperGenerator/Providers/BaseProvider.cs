namespace DapperGenerator.Providers;

public abstract partial class BaseProvider : IDatabaseProvider
{
    [GeneratedRegex(@"[^\w\d_]", RegexOptions.Compiled)]
    private static partial Regex CleanupRegex();

    private static readonly Regex _rxCleanUp = CleanupRegex();

    protected abstract string GetDatabaseSchemaQuery { get; }

    private static readonly string[] _reservedKeywords =
    [
        "abstract", "event", "new", "struct", "as", "explicit", "null",
        "switch", "base", "extern", "object", "this", "bool", "false", "operator", "throw",
        "break", "finally", "out", "true", "byte", "fixed", "override", "try", "case", "float",
        "params", "typeof", "catch", "for", "private", "uint", "char", "foreach", "protected",
        "ulong", "checked", "goto", "public", "unchecked", "class", "if", "readonly", "unsafe",
        "const", "implicit", "ref", "ushort", "continue", "in", "return", "using", "decimal",
        "int", "sbyte", "virtual", "default", "interface", "sealed", "volatile", "delegate",
        "internal", "short", "void", "do", "is", "sizeof", "while", "double", "lock",
        "stackalloc", "else", "long", "static", "enum", "namespace", "string",
    ];

    protected static string Normalize(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        str = _rxCleanUp.Replace(str, "_");

        if (char.IsDigit(str[0]) || _reservedKeywords.Contains(str))
        {
            str = "@" + str;
        }

        return str;
    }

    public abstract IReadOnlyCollection<TableInfo> ReadSchema(string connectionString);
}
