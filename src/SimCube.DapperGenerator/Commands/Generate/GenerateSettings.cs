namespace SimCube.DapperGenerator.Commands.Generate;

public sealed class GenerateSettings : CommandSettings
{
    [CommandOption("-o|--output <OUTPUT>")]
    [Required]
    [Description("Generated file path.")]
    public string OutputDirectory { get; set; } = null!;

    [CommandOption("--prefix <PREFIX>")]
    [Description("Class name prefix.")]
    public string? ClassPrefix { get; set; }

    [CommandOption("--suffix <SUFFIX>")]
    [Description("Class name suffix.")]
    public string? ClassSuffix { get; set; }

    [CommandOption("-n|--namespace <NAMESPACE>")]
    [Required]
    [Description("Set class namespace.")]
    public string Namespace { get; set; } = null!;

    [CommandOption("-c|--connection <CONNECTION>")]
    [Required]
    [Description("Set MSSQL connection string.")]
    public string ConnectionString { get; set; } = null!;

    [CommandOption("--validators")]
    [Description("Generate validator class (FluentValidation).")]
    public bool GenerateModelValidator { get; set; }

    [CommandOption("--include-json")]
    [Description("Include json property names when generating pocos.")]
    public bool IncludeJsonProperties { get; set; }
}
