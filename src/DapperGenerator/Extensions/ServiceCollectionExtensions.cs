namespace DapperGenerator.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterProviders(this IServiceCollection services)
    {
        services.AddKeyedSingleton<IDatabaseProvider, MsSqlServerProvider>(ProviderType.MsSqlServer.Name);

        return services;
    }

    public static IServiceCollection RegisterGenerators(this IServiceCollection services)
    {
        services.AddKeyedSingleton<ISourceGenerator, DapperPocoSourceGenerator>(GeneratorType.Poco.Name);
        services.AddKeyedSingleton<ISourceGenerator, DapperValidatorSourceGenerator>(GeneratorType.Validator.Name);

        return services;
    }

    public static IServiceCollection RegisterAnsiConsole(this IServiceCollection services)
    {
        services.AddSingleton(AnsiConsole.Console);

        return services;
    }
}
