namespace SimCube.DapperGenerator.Interfaces;

public interface IDatabaseProvider
{
    IReadOnlyCollection<TableInfo> ReadSchema(string connectionString);
}
