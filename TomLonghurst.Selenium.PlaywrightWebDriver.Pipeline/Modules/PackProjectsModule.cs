using EnumerableAsyncProcessor.Extensions;
using ModularPipelines;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Options;
using ModularPipelines.FileSystem;

namespace TomLonghurst.Selenium.PlaywrightWebDriver.Pipeline.Modules;

[DependsOn<PackageFilesRemovalModule>]
[DependsOn<NugetVersionGeneratorModule>]
[DependsOn<RunUnitTestsModule>]
public class PackProjectsModule : Module<CommandResult[]>
{
    protected override async Task<CommandResult[]> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var packageVersion = await context.GetModule<NugetVersionGeneratorModule>();

        var projectFile = (await context.Tools.Git.Information.GetRequiredInfoAsync(cancellationToken)).Root
            .AssertExists()
            .FindFile(x => x.Name == "TomLonghurst.Selenium.PlaywrightWebDriver.csproj")
            .AssertExists();

        return new List<CommandResult>
        {
            await PackV4(context, cancellationToken, projectFile, packageVersion),
            await PackV3(context, cancellationToken, projectFile, packageVersion)
        }.ToArray();
    }

    private static async Task<CommandResult> PackV4(IModuleContext context, CancellationToken cancellationToken, FilePath projectFile, ModuleResult<string> packageVersion)
    {
        return await context.Tools.DotNet.PackAsync(new DotNetPackOptions
        {
            ProjectSolution = projectFile.Path,
            Configuration = "Release",
            IncludeSource = true,
            Properties = new List<KeyValue>
            {
                ("PackageVersion", packageVersion.ValueOrDefault!),
                ("Version", packageVersion.ValueOrDefault!),
            },
        }, cancellationToken: cancellationToken);
    }

    private static async Task<CommandResult> PackV3(IModuleContext context, CancellationToken cancellationToken, FilePath projectFile, ModuleResult<string> packageVersion)
    {
        return await context.Tools.DotNet.PackAsync(new DotNetPackOptions
        {
            ProjectSolution = projectFile.Path,
            Configuration = "Release",
            IncludeSource = true,
            Properties = new List<KeyValue>
            {
                ("SeleniumVersion", "3"),
                ("PackageId", "TomLonghurst.Selenium.V3.PlaywrightWebDriver"),
                ("PackageVersion", packageVersion.ValueOrDefault!),
                ("Version", packageVersion.ValueOrDefault!),
            },
        }, cancellationToken: cancellationToken);
    }
}
