using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularPipelines;
using ModularPipelines.Extensions;
using TomLonghurst.Selenium.PlaywrightWebDriver.Pipeline.Modules;
using TomLonghurst.Selenium.PlaywrightWebDriver.Pipeline.Modules.LocalMachine;
using TomLonghurst.Selenium.PlaywrightWebDriver.Pipeline.Settings;

var pipelineBuilder = Pipeline.CreateBuilder(args);

pipelineBuilder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

pipelineBuilder.Services.Configure<NuGetSettings>(pipelineBuilder.Configuration.GetSection("NuGet"));

if (pipelineBuilder.Environment.IsDevelopment())
{
    pipelineBuilder
        .AddModule<CreateLocalNugetFolderModule>()
        .AddModule<AddLocalNugetSourceModule>()
        .AddModule<UploadPackagesToLocalNuGetModule>();
}
else
{
    pipelineBuilder.AddModule<UploadPackagesToNugetModule>();
}

await pipelineBuilder
    .AddModule<RunUnitTestsModule>()
    .AddModule<NugetVersionGeneratorModule>()
    .AddModule<PackProjectsModule>()
    .AddModule<PackageFilesRemovalModule>()
    .AddModule<PackagePathsParserModule>()
    .Build()
    .RunAsync();
