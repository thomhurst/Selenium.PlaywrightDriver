using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using File = ModularPipelines.FileSystem.File;

namespace TomLonghurst.Selenium.PlaywrightWebDriver.Pipeline.Modules;

public class RunUnitTestsModule : Module<List<CommandResult>>
{
    protected override async Task<List<CommandResult>?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var projectFile = context.Git()
            .RootDirectory
            .AssertExists()
            .FindFile(x => x.Name == "TomLonghurst.Selenium.PlaywrightWebDriver.Tests.csproj")
            .AssertExists();

        var results = new List<CommandResult>
        {
            await Run(context, cancellationToken, projectFile, true),
            await Run(context, cancellationToken, projectFile, false)
        };

        return results;
    }

    private static async Task<CommandResult> Run(IModuleContext context, CancellationToken cancellationToken,
        File unitTestProjectFile, bool isV3)
    {
        var dotNetTestOptions = new DotNetTestOptions
        {
            Project = unitTestProjectFile.Path,
        };

        if (isV3)
        {
            dotNetTestOptions.Properties = new[] { new KeyValue("SeleniumVersion", "3") };
        }

        return await context.DotNet().Test(dotNetTestOptions, cancellationToken: cancellationToken);
    }
}
