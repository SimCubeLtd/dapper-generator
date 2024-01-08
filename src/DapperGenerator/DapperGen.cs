namespace DapperGenerator;

public class DapperGen
{
    public static void StartupMessage()
    {
        var hideLogo = Environment.GetEnvironmentVariable("DAPPERGEN_NO_LOGO");

        if (!string.IsNullOrEmpty(hideLogo))
        {
            return;
        }

        AnsiConsole.Render(
            new FigletText("DapperGen")
                .Color(Color.HotPink));

        AnsiConsole.MarkupLine("[bold][green]Dapper Class and Validator Generation by Sim Cube Ltd[/][/]");
        AnsiConsole.WriteLine();
    }

    private static TypeRegistrar SetupRegistrations()
    {
        var registrations = new ServiceCollection();

        registrations
            .RegisterProviders()
            .RegisterGenerators()
            .RegisterAnsiConsole();

        return new(registrations);
    }

    public static ICommandApp Instance()
    {
        var app = new CommandApp(SetupRegistrations());

        app.Configure(config =>
        {
            config.Settings.ApplicationName = "dappergen";
            AddGenerateCommand(config);
        });

        return app;
    }

    private static void AddGenerateCommand(IConfigurator config) =>
        config.AddCommand<GenerateCommand>("generate")
            .WithDescription("Generate Dapper POCO models from a database schema")
            .WithExample(
            [
                "generate -c \"Server=localhost;Database=MyDatabase;User Id=sa;Password=Password123!\" -o \"./Models\"",
            ]);
}
