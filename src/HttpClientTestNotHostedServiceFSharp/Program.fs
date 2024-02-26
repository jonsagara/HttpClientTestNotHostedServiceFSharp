namespace HttpClientTestNotHostedServiceFSharp

open Microsoft.Extensions.DependencyInjection
open Serilog

module Program =

    [<EntryPoint>]
    let main argv =

        // The initial "bootstrap" logger is able to log errors during start-up. It's completely replaced by the
        //   logger configured in `UseSerilog()` in HostBuilderHelper, once configuration and dependency-injection
        //   have both been set up successfully.
        Log.Logger <- LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
            try
                //
                // Set up the generic host and DI.
                //

                let builder = ConsoleHost.createApplicationBuilder argv
                use host = builder.Build()

                use serviceScope = host.Services.CreateScope()
                let services = serviceScope.ServiceProvider


                //
                // Run the test service method.
                //

                let testSvc = services.GetRequiredService<TestService>()
                let html = 
                    testSvc.GetMicrosoftAsync() 
                    |> Async.AwaitTask 
                    |> Async.RunSynchronously

                let substringLength = if html.Length > 1_000 then 1_000 else html.Length
                printfn $"{html.Substring(0, substringLength)}"

                0
            with
            | ex -> 
                Log.Error(ex, "Unhandled exception in main")
                1
        finally
            Log.CloseAndFlush()
