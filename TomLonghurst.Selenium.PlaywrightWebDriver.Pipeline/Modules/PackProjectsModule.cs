using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnumerableAsyncProcessor.Extensions;
using Microsoft.Extensions.Logging;
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
        return await context.Git()
            .RootDirectory
            .AssertExists()
            .GetFiles(f => GetProjectsPredicate(f, context))
            .SelectAsync(f => Pack(context, cancellationToken, f, packageVersion), cancellationToken: cancellationToken)
            .ProcessOneAtATime();
    }

    private bool GetProjectsPredicate(File file, IPipelineContext context)
    {
        var path = file.Path;
        if (!path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (path.Contains("Tests", StringComparison.OrdinalIgnoreCase)
            || path.Contains("Pipeline", StringComparison.OrdinalIgnoreCase)
            || path.Contains("Example", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        context.Logger.LogInformation("Found File: {File}", path);
        return true;
    }
    
    private static async Task<CommandResult> Pack(IPipelineContext context, CancellationToken cancellationToken, File projectFile, ModuleResult<string> packageVersion)
    {
        return await context.DotNet().Pack(new DotNetPackOptions
        {
            ProjectSolution = projectFile.Path,
            Configuration = Configuration.Release,
            IncludeSource = !projectFile.Path.Contains("Analyzer"),
            NoRestore = true,
            Properties = new List<KeyValue>
            {
                ("PackageVersion", packageVersion.Value!),
                ("Version", packageVersion.Value!),
            },
        }, cancellationToken);
    }
}