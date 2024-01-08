namespace SimCube.DapperGenerator.Enums;

public class ProviderType(string name, int value) : SmartEnum<ProviderType, int>(name, value)
{
    public static ProviderType MsSqlServer { get; } = new(nameof(MsSqlServer), 1);
}
