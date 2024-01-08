namespace SimCube.DapperGenerator.Models;

public sealed class TableInfo
{
    public string Name { get; set; } = null!;
    public string Schema { get; set; }  = null!;
    public bool IsView { get; set; }
    public string CleanName { get; set; }  = null!;
    public List<ColumnInfo> Columns { get; set; } = [];
}
