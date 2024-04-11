using EnumerableAsyncProcessor.Extensions;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Extensions;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using File = ModularPipelines.FileSystem.File;

namespace TomLonghurst.Selenium.PlaywrightWebDriver.Pipeline.Modules;

[DependsOn<PackageFilesRemovalModule>]
[DependsOn<NugetVersionGeneratorModule>]
[DependsOn<RunUnitTestsModule>]
public class PackProjectsModule : Module<CommandResult[]>
{
    protected override async Task<CommandResult[]?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        var packageVersion = await GetModule<NugetVersionGeneratorModule>();

        var projectFile = context.Git()
            .RootDirectory
            .AssertExists()
            .FindFile(x => x.Name == "TomLonghurst.Selenium.PlaywrightWebDriver.csproj")
            .AssertExists();

        return new List<CommandResult>
        {
            await PackV4(context, cancellationToken, projectFile, packageVersion),
            await PackV3(context, cancellationToken, projectFile, packageVersion)
        }.ToArray();
    }

    private static async Task<CommandResult> PackV4(IPipelineContext context, CancellationToken cancellationToken, File projectFile, ModuleResult<string> packageVersion)
    {
        return await context.DotNet().Pack(new DotNetPackOptions
        {
            ProjectSolution = projectFile.Path,
            Configuration = Configuration.Release,
            IncludeSource = true,
            Properties = new List<KeyValue>
            {
                ("PackageVersion", packageVersion.Value!),
                ("Version", packageVersion.Value!),
            },
        }, cancellationToken);
    }
    
    private static async Task<CommandResult> PackV3(IPipelineContext context, CancellationToken cancellationToken, File projectFile, ModuleResult<string> packageVersion)
    {
        return await context.DotNet().Pack(new DotNetPackOptions
        {
            ProjectSolution = projectFile.Path,
            Configuration = Configuration.Release,
            IncludeSource = true,
            Properties = new List<KeyValue>
            {
                ("SeleniumVersion", "3"),
                ("PackageId", "TomLonghurst.Selenium.V3.PlaywrightWebDriver"),
                ("PackageVersion", packageVersion.Value!),
                ("Version", packageVersion.Value!),
            },
        }, cancellationToken);
    }
}