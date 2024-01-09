namespace SimCube.DapperGenerator.Generators;

public class DapperPocoSourceGenerator : BaseSourceGenerator
{
    public override async Task Generate(GenerateSettings config, IReadOnlyCollection<TableInfo> data)
    {
        foreach (var table in data)
        {
            SetupOutputDirectory(config, table, isEntity: true);

            var className = GetPrefixedSuffixed(config, table.CleanName);
            await CreateClass(table, className, config);
        }
    }

    protected override string GenClass(TableInfo properties, string className, GenerateSettings config)
    {
        _sourceBuilder.Clear();

        AddImports(properties, config);

        _sourceBuilder.AppendLine();
        _sourceBuilder.AppendLine($"namespace {config.Namespace.Pascalize()}.{properties.Schema.Pascalize()}.Entities;");
        _sourceBuilder.AppendLine();
        _sourceBuilder.AppendLine("/// <summary>");
        _sourceBuilder.AppendLine($"/// Schema: <b>{properties.Schema}</b>");
        _sourceBuilder.AppendLine($"/// Table Name: <b>{properties.Name}</b>");
        _sourceBuilder.AppendLine("/// </summary>");

        _sourceBuilder.AppendLine($"public class {className}Entity");
        _sourceBuilder.AppendLine("{");

        AddEachColumnAsProperty(properties, config);

        RemoveLastLineBreak();

        _sourceBuilder.AppendLine("}");
        _sourceBuilder.AppendLine();

        return _sourceBuilder.ToString();
    }

    private void AddImports(TableInfo properties, GenerateSettings config)
    {
        _sourceBuilder.AppendLine("using System;");

        if (config.IncludeJsonProperties)
        {
            _sourceBuilder.AppendLine("using System.Text.Json.Serialization;");
        }

        AddRelatedNamespaceImports(properties, config);
    }

    private void AddRelatedNamespaceImports(TableInfo properties, GenerateSettings config)
    {
        foreach (var schema in properties.Columns.Where(x => x.Relationship is not null).SelectMany(column => column.Relationship).Select(x => x.Schema).ToHashSet())
        {
            if (schema.Equals(properties.Schema, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            _sourceBuilder.AppendLine($"using {config.Namespace.Pascalize()}.{schema.Pascalize()}.Entities;");
        }
    }

    private void AddEachColumnAsProperty(TableInfo properties, GenerateSettings config)
    {
        foreach (var column in properties.Columns)
        {
            if (config.IncludeJsonProperties)
            {
                _sourceBuilder.AppendLine($"\t[JsonPropertyName(\"{JsonName(column.CleanName)}\")]");
            }

            switch (column.IsNullable)
            {
                case true:
                    _sourceBuilder.AppendLine($"\tpublic {column.Type}? {column.CleanName} {{ get; set; }}");
                    _sourceBuilder.AppendLine();
                    break;
                case false:
                    _sourceBuilder.AppendLine($"\tpublic {column.Type} {column.CleanName} {{ get; set; }} = default!;");
                    _sourceBuilder.AppendLine();
                    break;
            }

            if (column.Relationship is not {Count: > 0})
            {
                continue;
            }

            AppendRelationships(properties, column, config);
        }
    }

    private void AppendRelationships(TableInfo properties, ColumnInfo column, GenerateSettings config)
    {
        foreach (var relationshipRoot in column.Relationship)
        {
            _sourceBuilder.AppendLine("\t/// <summary>");
            _sourceBuilder.AppendLine($"\t/// Relationship:");
            _sourceBuilder.AppendLine($"\t/// Source Table: <b>[{properties.Schema}].[{properties.Name}]</b>");
            _sourceBuilder.AppendLine($"\t/// Target Table: <b>[{relationshipRoot.Schema}].[{relationshipRoot.TableName}]</b>");
            _sourceBuilder.AppendLine($"\t/// Foreign Key Property Name: <b>{column.Name}</b>.");
            _sourceBuilder.AppendLine("\t/// </summary>");

            if (config.IncludeJsonProperties)
            {
                _sourceBuilder.AppendLine($"\t[JsonPropertyName(\"{JsonName($"Ref{column.Name}")}\")]");
            }

            _sourceBuilder.AppendLine($"\tpublic {relationshipRoot.CleanName}Entity? Ref{column.Name} {{ get; set; }}");
            _sourceBuilder.AppendLine();
        }
    }
}
