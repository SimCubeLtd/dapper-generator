namespace SimCube.DapperGenerator.Commands.Generate;

public sealed class GenerateCommand(IServiceProvider serviceProvider, IAnsiConsole console) : AsyncCommand<GenerateSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateSettings settings)
    {
        console.MarkupLine("[blue]Generation started![/]");

        var sw = Stopwatch.StartNew();

        await using var scope = serviceProvider.CreateAsyncScope();

        // Only MsSql for now.
        var dbReader = scope.ServiceProvider.GetRequiredKeyedService<IDatabaseProvider>(ProviderType.MsSqlServer.Name);

        var data = ExtractData(settings, dbReader);

        if (data is null)
        {
            console.MarkupLine("[red]No data found[/]");
            return 1;
        }

        console.MarkupLine($"[blue]Extracted [green]{data.Count}[/] tables[/]");

        var dataGroupedBySchema = data.GroupBy(x => x.Schema).ToList();

        console.MarkupLine($"[blue]Found [green]{dataGroupedBySchema.Count}[/] schemas[/]");

        var rootOutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), settings.OutputDirectory);

        HandleExistingDirectory(rootOutputDirectory, settings);

        var generator = scope.ServiceProvider.GetRequiredKeyedService<ISourceGenerator>(GeneratorType.Poco.Name);

        foreach (var schemaData in dataGroupedBySchema)
        {
            var tables = schemaData.ToList();

            console.MarkupLine($"[blue]Generating schema [green]{schemaData.Key}[/] with [green]{tables.Count}[/] tables[/]");

            await generator.Generate(settings, tables);

            if (!settings.GenerateModelValidator)
            {
                continue;
            }

            var validatorGenerator = scope.ServiceProvider.GetRequiredKeyedService<ISourceGenerator>(GeneratorType.Validator.Name);
            await validatorGenerator.Generate(settings, tables);
        }

        sw.Stop();

        console.WriteLine();
        console.MarkupLine($"[green]All Done - Model generation completed successfully with output directory: [purple]{settings.OutputDirectory}[/][/];");
        console.MarkupLine($"[blue]Total Time Taken:[/] [purple]{sw.Elapsed.ToString()}[/]");

        return 0;
    }

    private void HandleExistingDirectory(string rootOutputDirectory, GenerateSettings input)
    {
        if (input.NonInteractive)
        {
            Directory.Delete(rootOutputDirectory, true);
            return;
        }

        if (Directory.Exists(rootOutputDirectory))
        {
            var prompt = $"[red]Directory '{rootOutputDirectory}' already exists, do you want to remove it first?[/]";
            var clean = console.Confirm(prompt);

            if (clean)
            {
                Directory.Delete(rootOutputDirectory, true);
            }
        }
    }

    private static IReadOnlyCollection<TableInfo>? ExtractData(GenerateSettings settings, IDatabaseProvider provider)
    {
        IReadOnlyCollection<TableInfo> data = null;

        AnsiConsole.Progress()
            .AutoClear(true)
            .Columns(
            [
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new ElapsedTimeColumn(),
                new SpinnerColumn(),
            ])
            .Start(
                context =>
                {
                    var gatherTask = context.AddTask("[blue]Reading schema[/]").IsIndeterminate();
                    data = provider.ReadSchema(settings.ConnectionString);
                    gatherTask.Value(100).StopTask();
                });

        return data;
    }
}
