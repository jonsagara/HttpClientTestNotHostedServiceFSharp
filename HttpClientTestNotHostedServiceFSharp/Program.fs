module Main

open System
open System.IO
open System.Linq
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open TestService

// Seems like a kludge, but we need a type to pass to the ILogger instance below.
type Program() = class end

[<EntryPoint>]
let main argv =

    // See: https://github.com/magnushammar/GenericHost.FSharp

    // Host configuration
    let hostConfig (configHost : IConfigurationBuilder) =
        configHost
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("hostsettings.json", optional = true)
            .AddEnvironmentVariables(prefix = "PREFIX_")
            .AddCommandLine(argv)
            |> ignore

    // App configuration
    let appConfig (context : HostBuilderContext) (configApp : IConfigurationBuilder) =
        configApp
            .AddJsonFile("appsettings.json", optional = true)
            .AddJsonFile(sprintf "appsettings.%s.json" context.HostingEnvironment.EnvironmentName, optional = true)
            .AddEnvironmentVariables(prefix = "PREFIX_")
            .AddCommandLine(argv)
            |> ignore

    // Services configuration
    let serviceConfig (context : HostBuilderContext) (services : IServiceCollection) =
        services
            .AddHttpClient()
            .AddTransient<TestService, TestService>()
            |> ignore

    // Logging configuration
    let loggingConfig (context : HostBuilderContext) (configLogging : ILoggingBuilder) =
        configLogging
            .AddConsole()
            .AddDebug()
            |> ignore

    let host = 
        HostBuilder()
            .ConfigureHostConfiguration(Action<IConfigurationBuilder> hostConfig)
            .ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> appConfig)
            .ConfigureServices(Action<HostBuilderContext, IServiceCollection> serviceConfig)
            .ConfigureLogging(Action<HostBuilderContext, ILoggingBuilder> loggingConfig)
            .Build()

    
    use serviceScope = host.Services.CreateScope()
    let services = serviceScope.ServiceProvider

    try
        let testSvc = services.GetRequiredService<TestService>()
        let html = testSvc.AsyncGetMicrosoft() |> Async.RunSynchronously
        printfn "%s" (String(html.Take(1_000).ToArray()))
    with
        | ex -> 
            let logger = services.GetRequiredService<ILogger<Program>>()
            Console.Error.WriteLine(sprintf "Unhandled exception %s" (ex.ToString()))
            logger.LogError(ex, "Unhandled exception");

    0 // return an integer exit code
