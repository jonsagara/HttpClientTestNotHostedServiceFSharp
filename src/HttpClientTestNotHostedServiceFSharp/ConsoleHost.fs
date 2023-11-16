namespace HttpClientTestNotHostedServiceFSharp

open System
open System.IO
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Serilog
open Sagara.Core.Logging.Serilog

module ConsoleHost =

    let private configureSerilog (builder : IHostApplicationBuilder) (services : IServiceProvider) (loggerConfig : LoggerConfiguration) =
        // This is the .exe path in bin/{configuration}/{tfm}/
        let currentExeDir = Directory.GetCurrentDirectory()

        // Log to the project directory.
        let logDir = 
            Path.Combine(currentExeDir, @"..\..\..") 
            |> Path.GetFullPath
        Log.Logger.Information("Logging directory: {logDir}", logDir)

        let logFilePathFormat = Path.Combine(logDir, "Logs", "log.txt")

        // Always write to a rolling file.
        loggerConfig = loggerConfig
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.With<UtcTimestampEnricher>()
            .WriteTo.Console()
            .WriteTo.File(
                path = logFilePathFormat,
                outputTemplate = "{UtcTimestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] [{SourceContext:l}] {Message}{NewLine}{Exception}", 
                rollingInterval = RollingInterval.Day, 
                retainedFileTimeLimit = TimeSpan.FromDays(7.0)
                ) |> ignore

    
    let createApplicationBuilder (args : string[]) =
        let builderSettings = HostApplicationBuilderSettings(Args = args)

        // Load EnvironmentName from hostsettings.json.
        builderSettings.Configuration <- new ConfigurationManager()
        builderSettings.Configuration.AddJsonFile(path = "hostsettings.json", optional = true) |> ignore

        let builder = HostApplicationBuilder(builderSettings);


        //
        // Configure services
        //

        // Let Serilog take over logging responsibilities.
        builder.UseSerilog(configureSerilog) |> ignore

        Log.Information("Environment: {environmentName}", builder.Environment.EnvironmentName)

        builder.Services.AddScoped<TestService>() |> ignore

        builder.Services
            .AddHttpClient()
            |> ignore

        builder

