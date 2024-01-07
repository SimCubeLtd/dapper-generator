## Introduction
**DapperGen** is a .net global tool for generating [Dapper](https://github.com/DapperLib/Dapper) POCO models from a database schema. It is a command line tool that can be used to generate POCO models from a database schema. It is designed to be used as part of a build process to generate models from a database schema.
It also supports generating [FluentValidation](https://fluentvalidation.net/) classes for the generated POCOs.

It is Very fast - a single query collects all data needed for generation


```bash
  ____                                           ____
 |  _ \    __ _   _ __    _ __     ___   _ __   / ___|   ___   _ __
 | | | |  / _` | | '_ \  | '_ \   / _ \ | '__| | |  _   / _ \ | '_ \
 | |_| | | (_| | | |_) | | |_) | |  __/ | |    | |_| | |  __/ | | | |
 |____/   \__,_| | .__/  | .__/   \___| |_|     \____|  \___| |_| |_|
                 |_|     |_|
USAGE:
    dappergen [OPTIONS] <COMMAND>

EXAMPLES:
    dappergen generate -c "Server=localhost;Database=MyDatabase;User Id=sa;Password=Password123!" -o "./Models"

OPTIONS:
    -h, --help       Prints help information
    -v, --version    Prints version information

COMMANDS:
    generate    Generate Dapper POCO models from a database schema
```

## Options
```bash
    -h, --help                       Prints help information
    -o, --output <OUTPUT>            Generated file path
        --prefix <PREFIX>            Class name prefix
        --suffix <SUFFIX>            Class name suffix
    -n, --namespace <NAMESPACE>      Set class namespace
    -c, --connection <CONNECTION>    Set MSSQL connection string
        --validators                 Generate validator class (FluentValidation)
        --include-json               Include json property names when generating pocos

```

## Generated POCOs
```csharp
using System;
using System.Text.Json.Serialization;

namespace DapperTest.Commercial.Entities;

/// <summary>
/// Schema: <b>Commercial</b>
/// Table Name: <b>ProjectTimeSheet</b>
/// </summary>
public class ProjectTimeSheet
{
	[JsonPropertyName("projectTimeSheetID")]
	public int ProjectTimeSheetID { get; set; }

	[JsonPropertyName("employeeID")]
	public int EmployeeID { get; set; }

	[JsonPropertyName("sectionID")]
	public int SectionID { get; set; }

	/// <summary>
	/// Relationship:
	/// Source Table: <b>[Commercial].[ProjectTimeSheet]</b>
	/// Target Table: <b>[Commercial].[ProjectSections]</b>
	/// Foreign Key Property Name: <b>SectionID</b>.
	/// </summary>
	[JsonPropertyName("refSectionID")]
	public ProjectSection RefSectionID { get; set; }

	[JsonPropertyName("nominalID")]
	public int NominalID { get; set; }

	/// <summary>
	/// Relationship:
	/// Source Table: <b>[Commercial].[ProjectTimeSheet]</b>
	/// Target Table: <b>[Accounts].[NominalCodes]</b>
	/// Foreign Key Property Name: <b>NominalID</b>.
	/// </summary>
	[JsonPropertyName("refNominalID")]
	public NominalCode RefNominalID { get; set; }

	[JsonPropertyName("date")]
	public DateTime Date { get; set; }

	[JsonPropertyName("hours")]
	public decimal Hours { get; set; }

	[JsonPropertyName("rate")]
	public decimal Rate { get; set; }

	[JsonPropertyName("comment")]
	public string Comment { get; set; }

	[JsonPropertyName("createdBy")]
	public int CreatedBy { get; set; }

	[JsonPropertyName("createdDate")]
	public DateTime CreatedDate { get; set; }
}
```

## Generated Validator Class
```csharp
using FluentValidation;
using DapperTest.Commercial.Entities;

namespace DapperTest.Commercial.Validators;

public class ProjectTimeSheetValidator : AbstractValidator<ProjectTimeSheet>
{
	public ProjectTimeSheetValidator()
	{
		RuleFor(entity => entity.ProjectTimeSheetID).NotEmpty();

		RuleFor(entity => entity.EmployeeID).NotEmpty();

		RuleFor(entity => entity.SectionID).NotEmpty();

		RuleFor(entity => entity.NominalID).NotEmpty();

		RuleFor(entity => entity.Date).NotEmpty();

		RuleFor(entity => entity.Hours).NotEmpty();

		RuleFor(entity => entity.Rate).NotEmpty();

		RuleFor(entity => entity.Comment).MaximumLength(250);

		RuleFor(entity => entity.CreatedBy).NotEmpty();

		RuleFor(entity => entity.CreatedDate).NotEmpty();
	}
}
```

## That's cool but I just want the sql query used to generate the POCOs - sure thing :simple_smile:
```sql
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
```

## Based on
Dapper Class Generator by [Baranovskis](https://github.com/baranovskis/dapper-class-generator/tree/main)
