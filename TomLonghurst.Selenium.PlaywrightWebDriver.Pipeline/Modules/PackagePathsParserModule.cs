using ModularPipelines;
using ModularPipelines.Context;
using ModularPipelines.FileSystem;

namespace TomLonghurst.Selenium.PlaywrightWebDriver.Pipeline.Modules;

[DependsOn<PackProjectsModule>]
public class PackagePathsParserModule : Module<List<FilePath>>
{
    protected override async Task<List<FilePath>> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var packPackagesModuleResult = await context.GetModule<PackProjectsModule>();

        return packPackagesModuleResult.ValueOrDefault!
            .Select(x => x.StandardOutput)
            .Select(x => x.Split("Successfully created package '")[1])
            .Select(x => x.Split("'.")[0])
            .Select(x => new FilePath(x))
            .ToList();
    }
}
