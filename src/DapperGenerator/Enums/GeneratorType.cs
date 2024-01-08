namespace DapperGenerator.Enums;

public class GeneratorType(string name, int value) : SmartEnum<GeneratorType, int>(name, value)
{
    public static GeneratorType Poco { get; } = new(nameof(Poco), 1);
    public static GeneratorType Validator { get; } = new(nameof(Validator), 2);
}
