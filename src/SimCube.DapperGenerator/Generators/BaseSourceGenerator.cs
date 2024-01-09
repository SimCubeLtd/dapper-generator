namespace SimCube.DapperGenerator.Generators;

public abstract class BaseSourceGenerator : ISourceGenerator
{
    protected readonly StringBuilder _sourceBuilder = new();
    private string _outputDirectory = null!;

    public abstract Task Generate(GenerateSettings config, IReadOnlyCollection<TableInfo> tableInfo);

    protected abstract string GenClass(TableInfo properties, string className, GenerateSettings config);

    protected static string GetPrefixedSuffixed(GenerateSettings config, string name) => $"{config.ClassPrefix ?? string.Empty}{name}{config.ClassSuffix ?? string.Empty}";

    protected void SetupOutputDirectory(GenerateSettings config, TableInfo? tableInfo = null, bool? isEntity = null, bool? isValidator = null)
    {
        if (isValidator is true && isEntity is true)
        {
            throw new ArgumentException("Cannot be both a validator and an entity");
        }

        _outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), config.OutputDirectory);

        if (tableInfo is not null)
        {
            _outputDirectory = Path.Combine(_outputDirectory, tableInfo.Schema);
        }

        if (isValidator is true)
        {
            _outputDirectory = Path.Combine(_outputDirectory, "Validators");
        }

        if (isEntity is true)
        {
            _outputDirectory = Path.Combine(_outputDirectory, "Entities");
        }

        if (!Directory.Exists(_outputDirectory))
        {
            Directory.CreateDirectory(_outputDirectory);
        }
    }

    protected async Task CreateClass(TableInfo properties, string className, GenerateSettings config)
    {
        string model = GenClass(
            properties,
            className,
            config);

        string fileName = $"{className}.cs";

        string filePath = Path.Combine(
            _outputDirectory,
            fileName);

        await File.WriteAllTextAsync(filePath, model);
    }

    protected static string JsonName(string fieldName)
    {
        string jsonName = fieldName;
        string first = jsonName[..1].ToLower();
        jsonName = string.Concat(first, jsonName.AsSpan(1, jsonName.Length - 1));

        return jsonName;
    }

    protected void RemoveLastLineBreak() => _sourceBuilder.Remove(_sourceBuilder.Length - 1, 1);
}
