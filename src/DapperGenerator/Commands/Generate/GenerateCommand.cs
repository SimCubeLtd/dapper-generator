namespace DapperGenerator.Commands.Generate;

public class GenerateCommand : AsyncCommand<GenerateSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateSettings settings)
    {
        AnsiConsole.MarkupLine("[blue]Generation started![/]");

        var sw = Stopwatch.StartNew();

        AnsiConsole.MarkupLine("[blue]Reading schema...[/]");

        var data = DbReader.ReadSchema(settings.ConnectionString);

        AnsiConsole.MarkupLine($"[blue]Extracted [green]{data.Count}[/] tables[/]");

        var dataGroupedBySchema = data.GroupBy(x => x.Schema).ToList();

        AnsiConsole.MarkupLine($"[blue]Found [green]{dataGroupedBySchema.Count}[/] schemas[/]");

        var rootOutputDirectory = Path.Combine(AppContext.BaseDirectory, settings.OutputDirectory);

        if (Directory.Exists(rootOutputDirectory))
        {
            var prompt = $"[red]Directory '{rootOutputDirectory}' already exists, do you want to remove it first?[/]";
            var clean = AnsiConsole.Confirm(prompt);

            if (clean)
            {
                Directory.Delete(rootOutputDirectory, true);
            }
        }

        var generator = new DapperPocoSourceGenerator();
        var validatorGenerator = new DapperValidatorSourceGenerator();

        foreach (var schemaData in dataGroupedBySchema)
        {
            var tables = schemaData.ToList();

            AnsiConsole.MarkupLine($"[blue]Generating schema [green]{schemaData.Key}[/] with [green]{tables.Count}[/] tables[/]");

            await generator.Generate(settings, tables);

            if (settings.GenerateModelValidator)
            {
                await validatorGenerator.Generate(settings, tables);
            }
        }

        sw.Stop();

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]All Done - Model generation completed successfully with output directory: [purple]{settings.OutputDirectory}[/][/];");
        AnsiConsole.MarkupLine($"Total Time Taken: [purple]{sw.Elapsed.ToString()}[/]");

        return 0;
    }
}
