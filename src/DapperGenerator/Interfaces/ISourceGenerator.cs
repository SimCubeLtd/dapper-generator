namespace DapperGenerator.Interfaces;

public interface ISourceGenerator
{
    Task Generate(GenerateSettings config, IReadOnlyCollection<TableInfo> tableInfo);
}
