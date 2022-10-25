namespace HttpClientTestNotHostedServiceFSharp

module HostBuilderHelper =

    open System
    open System.IO
    open Microsoft.Extensions.Configuration
    open Microsoft.Extensions.DependencyInjection
    open Microsoft.Extensions.Hosting
    open Microsoft.Extensions.Logging
    open Serilog
    open Serilog.Events


    /// Host configuration
    let private configureHost (configBuilder : IConfigurationBuilder) (argv : string[]) =
        configBuilder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("hostsettings.json", optional = true)
            //            // Analogous to ASPNETCORE_WHATEVER, except it's PREFIX_WHATEVER
            //            .AddEnvironmentVariables(prefix = "PREFIX_")
            .AddCommandLine(argv)
            |> ignore

    /// App configuration
    let private configureApp (context : HostBuilderContext) (configApp : IConfigurationBuilder) (argv : string[]) =
        configApp
            .AddJsonFile("appsettings.json", optional = true)
            .AddJsonFile($"appsettings.%s{context.HostingEnvironment.EnvironmentName}.json", optional = true)
            //            // Analogous to ASPNETCORE_WHATEVER, except it's PREFIX_WHATEVER
            //            .AddEnvironmentVariables(prefix = "PREFIX_")
            .AddCommandLine(argv)
            |> ignore

    /// Services configuration
    let private configureServices (context : HostBuilderContext) (services : IServiceCollection) =
        Log.Information("Environment: {environmentName}", context.HostingEnvironment.EnvironmentName)

        services.AddScoped<TestService>() |> ignore

        services
            .AddHttpClient()
            |> ignore

    /// Logging configuration
    let private configureLogging (context : HostBuilderContext) (configLogging : ILoggingBuilder) =
        configLogging
            //.AddConsole()
            //.AddDebug()
            |> ignore

    // Configure Serilog for logging.
    let private configureSerilog (context : HostBuilderContext) (services : IServiceProvider) (loggerConfig : LoggerConfiguration) =

        // This is the .exe path in bin/{configuration}/{tfm}/
        let currentExeDir = Directory.GetCurrentDirectory();

        // Log to the project directory.
        let logDir = 
            Path.Combine(currentExeDir, @"..\..\..") 
            |> Path.GetFullPath
        Log.Information("Logging directory: {logDir}", logDir);

        // Serilog is our application logger. Default to Verbose. If we need to control this dynamically at some point
        //   in the future, we can: https://nblumhardt.com/2014/10/dynamically-changing-the-serilog-level/

        let logFilePathFormat = Path.Combine(logDir, "Logs", "log.txt");

        // Always write to a rolling file.
        loggerConfig = loggerConfig
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.With<UtcTimestampEnricher>()
            .WriteTo.Console()
            .WriteTo.File(logFilePathFormat, LogEventLevel.Verbose, "{UtcTimestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] [{SourceContext:l}] {Message}{NewLine}{Exception}", null, 1073741824L, null, false, false, Nullable(), RollingInterval.Day)
            |> ignore

    /// Configure and build an IHost.
    let buildHost (argv : string[]) =
        HostBuilder()
            .ConfigureHostConfiguration(Action<IConfigurationBuilder> (fun cb -> configureHost cb argv))
            .ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> (fun ctx cb -> configureApp ctx cb argv))
            .ConfigureServices(configureServices)
            .ConfigureLogging(configureLogging)
            .UseSerilog(configureSerilog)
            .Build()

