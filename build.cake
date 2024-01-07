#addin "nuget:?package=Cake.MinVer&version=3.0.0"

using System.Text.Json;

var target = Argument("Target", "Default");

var configuration =
    HasArgument("Configuration") ? Argument<string>("Configuration") :
    EnvironmentVariable("Configuration", "Release");

var shouldPack = HasArgument("pack") ? Argument<bool>("pack") : false;

var settings = new MinVerSettings()
{
    AutoIncrement = MinVerAutoIncrement.Minor,
    DefaultPreReleasePhase = "preview",
    MinimumMajorMinor = "0.1",
    TagPrefix = "v",
    Verbosity = MinVerVerbosity.Trace,
};

var version = MinVer(settings);

Task("Clean")
    .Description("Cleans the artifacts, bin and obj directories.")
    .Does(() =>
    {
        DeleteDirectories(GetDirectories("**/bin"), new DeleteDirectorySettings() { Force = true, Recursive = true });
        DeleteDirectories(GetDirectories("**/obj"), new DeleteDirectorySettings() { Force = true, Recursive = true });
        CleanDirectory("./artifacts");
    });

Task("Restore")
    .Description("Restores NuGet packages.")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetRestore();
    });

Task("Build")
    .Description("Builds the solution.")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetBuild(
            ".",
            new DotNetBuildSettings()
            {
                Configuration = configuration,
                NoRestore = true,
                ArgumentCustomization = args =>
                  args
                  .Append($"-p:Version={version}")
                  .Append($"-p:InformationalVersion={version}"),
            });
    });

Task("Pack")
    .Description("Packs the Required Project")
    .IsDependentOn("Build")
    .WithCriteria(shouldPack)
    .Does(() =>
        {
            DotNetPack("src/DapperGenerator/DapperGenerator.csproj",
             new DotNetPackSettings()
                        {
                            NoBuild = true,
                            NoRestore = true,
                            NoLogo = true,
                            OutputDirectory = "./artifacts",
                            Verbosity = DotNetVerbosity.Minimal,
                            Configuration = configuration,
                            ArgumentCustomization = builder => builder.Append($"-p:PackageVersion={version}")
                        });
        });

Task("Default")
    .Description("Cleans, restores NuGet packages, builds the solution and then runs unit tests.")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Pack");

RunTarget(target);
