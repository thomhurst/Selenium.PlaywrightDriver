using ModularPipelines;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Options;
using ModularPipelines.FileSystem;

namespace TomLonghurst.Selenium.PlaywrightWebDriver.Pipeline.Modules;

public class RunUnitTestsModule : Module<List<CommandResult>>
{
    protected override async Task<List<CommandResult>> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var projectFile = (await context.Tools.Git.Information.GetRequiredInfoAsync(cancellationToken)).Root
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
        FilePath unitTestProjectFile, bool isV3)
    {
        var dotNetTestOptions = new DotNetTestVstestOptions
        {
            ProjectSolution = unitTestProjectFile.Path,
            Properties = isV3 ? [new KeyValue("SeleniumVersion", "3")] : null,
        };

        return await context.Tools.DotNet.TestVstestAsync(dotNetTestOptions, cancellationToken: cancellationToken);
    }
}
