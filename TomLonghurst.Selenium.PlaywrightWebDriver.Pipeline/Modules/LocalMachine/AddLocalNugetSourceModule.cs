using ModularPipelines;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Exceptions;

namespace TomLonghurst.Selenium.PlaywrightWebDriver.Pipeline.Modules.LocalMachine;

[DependsOn<CreateLocalNugetFolderModule>]
public class AddLocalNugetSourceModule : Module<CommandResult?>
{
    /// <inheritdoc/>
    protected override async Task<CommandResult?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var localNugetPathResult = await context.GetModule<CreateLocalNugetFolderModule>();

        try
        {
            return await context.Tools.DotNet.NuGet.Add.SourceAsync(new DotNetNuGetAddSourceOptions(localNugetPathResult.ValueOrDefault!.Path)
            {
                Name = "ModularPipelinesLocalNuGet",
            }, cancellationToken: cancellationToken);
        }
        catch (CommandException ex) when (ex.Result.StandardOutput.Contains("The name specified has already been added to the list of available package sources"))
        {
            return null;
        }
    }
}
