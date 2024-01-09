namespace SimCube.DapperGenerator.Generators;

public class DapperValidatorSourceGenerator : BaseSourceGenerator
{
    public override async Task Generate(GenerateSettings config, IReadOnlyCollection<TableInfo> data)
    {
        foreach (var table in data)
        {
            SetupOutputDirectory(config, table, isValidator: true);

            var className = GetPrefixedSuffixed(config, table.CleanName + "Validator");
            await CreateClass(table, className, config);
        }
    }

    protected override string GenClass(TableInfo properties, string className, GenerateSettings config)
    {
        _sourceBuilder.Clear();

        _sourceBuilder.AppendLine("using FluentValidation;");
        _sourceBuilder.AppendLine($"using {config.Namespace.Pascalize()}.{properties.Schema.Pascalize()}.Entities;");
        _sourceBuilder.AppendLine();
        _sourceBuilder.AppendLine($"namespace {config.Namespace.Pascalize()}.{properties.Schema.Pascalize()}.Validators;");
        _sourceBuilder.AppendLine();
        _sourceBuilder.AppendLine($"public class {className} : AbstractValidator<{GetPrefixedSuffixed(config, properties.CleanName)}Entity>");
        _sourceBuilder.AppendLine("{");
        _sourceBuilder.AppendLine($"\tpublic {className}()");
        _sourceBuilder.AppendLine("\t{");
        foreach (var column in properties.Columns)
        {
            if (!column.IsNullable && column.Type.Equals("string", StringComparison.OrdinalIgnoreCase))
            {
                _sourceBuilder.AppendLine($"\t\tRuleFor(entity => entity.{column.CleanName}).NotNull().NotEmpty();");
                _sourceBuilder.AppendLine();
            }

            if (!column.IsNullable && !column.Type.Equals("string", StringComparison.OrdinalIgnoreCase))
            {
                _sourceBuilder.AppendLine($"\t\tRuleFor(entity => entity.{column.CleanName}).NotNull();");
                _sourceBuilder.AppendLine();
            }

            if (column.MaximumLength > 0 && column.Type.Equals("byte[]", StringComparison.OrdinalIgnoreCase))
            {
                _sourceBuilder.AppendLine($"\t\tRuleFor(entity => entity.{column.CleanName}).MaximumLength({column.MaximumLength});");
                _sourceBuilder.AppendLine();
            }
        }

        RemoveLastLineBreak();

        _sourceBuilder.AppendLine("\t}");
        _sourceBuilder.AppendLine("}");

        return _sourceBuilder.ToString();
    }
}
