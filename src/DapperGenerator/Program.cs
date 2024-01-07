AnsiConsole.Render(
    new FigletText("DapperGen")
        .Color(Color.Green));

var app = new CommandApp();

app.Configure(config =>
{
    config.Settings.ApplicationName = "dappergen";
    config.AddCommand<GenerateCommand>("generate")
        .WithDescription("Generate Dapper POCO models from a database schema")
        .WithExample(
        [
            "generate -c \"Server=localhost;Database=MyDatabase;User Id=sa;Password=Password123!\" -o \"./Models\"",
        ]);

});

return await app.RunAsync(args);
