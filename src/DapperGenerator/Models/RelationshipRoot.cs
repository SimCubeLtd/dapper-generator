namespace DapperGenerator.Models;

public class RelationshipRoot
{
    public string TableName { get; set; } = null!;
    public string Schema { get; set; } = null!;
    public string CleanName { get; set; } = null!;
    public List<Relationship> Relationships { get; set; } = null!;
}
