using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Exceptions;
using ModularPipelines.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;

namespace TomLonghurst.Selenium.PlaywrightWebDriver.Pipeline.Modules.LocalMachine;

[DependsOn<CreateLocalNugetFolderModule>]
public class AddLocalNugetSourceModule : Module<CommandResult>
{
    /// <inheritdoc/>
    protected override async Task<CommandResult?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var localNugetPathResult = await context.GetModule<CreateLocalNugetFolderModule>();

        try
        {
            return await context.DotNet().Nuget.Add.Source(new DotNetNugetAddSourceOptions
            {
                Packagesourcepath = localNugetPathResult.ValueOrDefault!.Path,
                Name = "ModularPipelinesLocalNuGet",
            }, cancellationToken: cancellationToken);
        }
        catch (CommandException ex) when (ex.StandardOutput.Contains("The name specified has already been added to the list of available package sources"))
        {
            return null;
        }
    }
}
