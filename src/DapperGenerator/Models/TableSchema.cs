namespace DapperGenerator.Models;

public sealed class TableSchema
{
    public string SchemaName { get; set; } = null!;
    public string TableName { get; set; } = null!;
    public string TableType { get; set; } = null!;
    public string ColumnName { get; set; } = null!;
    public int ColumnPosition { get; set; }
    public bool IsNullable { get; set; }
    public string DataType { get; set; } = null!;
    public int MaximumLength { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsIdentity { get; set; }
    public bool IsComputed { get; set; }

    public string? RelationshipXML { get; set; }
}
