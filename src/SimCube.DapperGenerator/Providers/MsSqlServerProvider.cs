namespace SimCube.DapperGenerator.Providers;

public sealed class MsSqlServerProvider(IAnsiConsole console) : BaseProvider
{
    protected override string GetDatabaseSchemaQuery =>
        """
          SELECT
            SchemaName = SCHEMA_NAME(t.schema_id),
            TableName = t.[name],
            ColumnName = c.[name],
            ColumnPosition = c.column_id,
            IsNullable = c.is_nullable,
            DataType = ty.[name],
            MaximumLength = COLUMNPROPERTY(c.object_id, c.[name], 'CharMaxLen'),
            IsIdentity = c.is_identity,
            IsComputed = c.is_computed,
            IsPrimary = ISNULL(pk.is_primary_key, 0),
            RelationshipXML = fk.Relationship
        FROM sys.tables t
        INNER JOIN sys.columns c ON c.object_id = t.object_id
        LEFT JOIN sys.types ty ON ty.user_type_id = c.user_type_id
        OUTER APPLY (
            SELECT i.is_primary_key
            FROM sys.indexes i
            INNER JOIN sys.index_columns ic ON ic.object_id = i.object_id
                AND ic.index_id = i.index_id
                AND ic.column_id = c.column_id
            WHERE i.object_id = t.object_id
                AND i.is_primary_key = 1
        ) pk
        OUTER APPLY (
            SELECT
                SchemaName = SCHEMA_NAME(t2.schema_id),
                TableName = t2.[name],
                Relationships = (
                    SELECT
                        ParentColumnName = pc.[name],
                        ReferenceColumnName = rc.[name]
                    FROM sys.foreign_key_columns fkca
                    INNER JOIN sys.columns pc ON fkca.parent_column_id = pc.column_id
                        AND fkca.parent_object_id = pc.object_id
                    INNER JOIN sys.columns rc ON fkca.referenced_column_id = rc.column_id
                        AND fkca.referenced_object_id = rc.object_id
                    WHERE fkca.constraint_object_id = fkc.constraint_object_id
                        AND fkca.parent_object_id = fkc.parent_object_id
                    FOR XML PATH('Relationship'), TYPE
                )
            FROM sys.foreign_key_columns fkc
            INNER JOIN sys.tables t2 ON t2.object_id = fkc.referenced_object_id
            WHERE fkc.parent_column_id = c.column_id
                AND fkc.parent_object_id = c.object_id
                AND fkc.constraint_column_id = 1
            FOR XML PATH('RelationshipRoot')
        ) fk (Relationship)
        ORDER BY SchemaName, TableName, ColumnPosition
        """;


    public override IReadOnlyCollection<TableInfo> ReadSchema(string connectionString)
    {
        var result = new List<TableInfo>();

        using var connection = new SqlConnection(connectionString);
        var queryResult = connection.Query<TableSchema>(GetDatabaseSchemaQuery)
            .ToList();

        var groupedTables = queryResult.GroupBy(
                x => new
                {
                    x.SchemaName, x.TableName, x.TableType,
                })
            .ToList();

        foreach (var tbl in groupedTables.Select(
                     table => new TableInfo
                     {
                         Name = table.Key.TableName,
                         Schema = table.Key.SchemaName,
                         IsView = string.Compare(table.Key.TableType, "View", StringComparison.OrdinalIgnoreCase) == 0,
                     }))
        {
            tbl.CleanName = Normalize(tbl.Name).Pascalize()?.Singularize();

            foreach (var res in queryResult
                         .Where(x => x.TableName == tbl.Name && x.SchemaName == tbl.Schema)
                         .OrderBy(x => x.ColumnPosition))
            {
                var col = new ColumnInfo
                {
                    Name = res.ColumnName,
                    Type = ConvertPropertyType(res.DataType),
                    IsIdentity = res.IsIdentity,
                    IsPrimary = res.IsPrimary,
                    IsComputed = res.IsComputed,
                    IsNullable = res.IsNullable,
                    MaximumLength = res.MaximumLength,
                };

                col.CleanName = Normalize(col.Name).Pascalize();

                if (!string.IsNullOrEmpty(res.RelationshipXML))
                {
                    try
                    {
                        var doc = XDocument.Parse("<Root>" + res.RelationshipXML + "</Root>");
                        col.Relationship = [];

                        foreach (var relationshipRootElement in doc.Descendants("RelationshipRoot"))
                        {
                            var relationshipRoot = new RelationshipRoot
                            {
                                TableName = relationshipRootElement.Element("TableName")?.Value,
                                Schema = relationshipRootElement.Element("SchemaName")?.Value,
                            };

                            relationshipRoot.CleanName = Normalize(relationshipRoot.TableName).Pascalize()?.Singularize();

                            relationshipRoot.Relationships = relationshipRootElement
                                .Element("Relationships")?
                                .Elements("Relationship")
                                .Select(
                                    r => new Relationship
                                    {
                                        ParentColumnName = r.Element("ParentColumnName")?.Value,
                                        ReferenceColumnName = r.Element("ReferenceColumnName")?.Value
                                    })
                                .ToList();

                            col.Relationship.Add(relationshipRoot);
                        }
                    }
                    catch (Exception ex)
                    {
                        console.MarkupLine($"[red]Error deserializing XML: {ex.Message}[/]");
                        console.MarkupLine($"[blue]XML content was: {res.RelationshipXML}[/]");
                    }
                }

                if (col.Relationship is {Count: > 0})
                {
                    foreach (var relationshipRoot in col.Relationship)
                    {
                        relationshipRoot.CleanName = Normalize(relationshipRoot.TableName).Pascalize()?.Singularize();
                    }
                }

                tbl.Columns.Add(col);
            }

            result.Add(tbl);
        }

        return result;
    }

    private static string ConvertPropertyType(string type) => type switch
    {
        "bigint" => "long",
        "smallint" => "short",
        "int" => "int",
        "uniqueidentifier" => "Guid",
        "smalldatetime" => "DateTime",
        "datetime" => "DateTime",
        "datetime2" => "DateTime",
        "date" => "DateTime",
        "time" => "DateTime",
        "datetimeoffset" => "DateTimeOffset",
        "float" => "double",
        "real" => "float",
        "numeric" => "decimal",
        "smallmoney" => "decimal",
        "decimal" => "decimal",
        "money" => "decimal",
        "tinyint" => "byte",
        "bit" => "bool",
        "image" => "byte[]",
        "binary" => "byte[]",
        "varbinary" => "byte[]",
        "timestamp" => "byte[]",
        "geography" => "Microsoft.SqlServer.Types.SqlGeography",
        "geometry" => "Microsoft.SqlServer.Types.SqlGeometry",
        _ => "string",
    };
}
