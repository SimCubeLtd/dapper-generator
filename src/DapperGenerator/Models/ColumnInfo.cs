namespace DapperGenerator.Models;

public sealed class ColumnInfo
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public bool IsNullable { get; set; }
    public int MaximumLength { get; set; }
    public bool IsIdentity { get; set; }
    public bool IsComputed { get; set; }
    public bool IsPrimary { get; set; }
    public string? CleanName { get; set; }
    public List<RelationshipRoot>? Relationship { get; set; }
}
