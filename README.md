# Use `HttpClient` via `IHttpClientFactory` in a .NET Core console app without implementing `IHostedService` #

> If you're looking for the C# version, go [here](https://github.com/jonsagara/HttpClientTestNotHostedService).

Not that there's anything wrong with `IHostedService`, but sometimes you just want a plain old console app without 
having to implement another interface just so that you can inject and use `IHttpClientFactory`.

By using the new `HostApplicationBuilder` pattern (encapsulated in the `ConsoleHost` class), there is much less
ceremony involved with setting up the Generic Host `HostBuilder` so that you can inject `IHttpClientFactory` 
into your classes. Beyond that, you merely create an `IServiceScope` to make everything work:

```fsharp
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
let html = testSvc.GetMicrosoftAsync() |> Async.AwaitTask |> Async.RunSynchronously

let substringLength = if html.Length > 1_000 then 1_000 else html.Length
printfn $"{html.Substring(0, substringLength)}"
```

This code creates an `IServiceScope` in `main`, and then uses that to get our registered `TestService` class, which returns the HTML of https://www.microsoft.com
as a string. We then display the first 1,000 characters.

The `TestService` class looks like this:

```fsharp
type TestService (httpClientFactory : IHttpClientFactory) =

    static let _logger = Log.Logger.ForContext<TestService>()

    let _httpClient = httpClientFactory.CreateClient()

    member this.GetMicrosoftAsync() =
        task {
            _logger.Information("Making HTTP request...")
            use requestMsg = new HttpRequestMessage(HttpMethod.Get, "https://www.microsoft.com")
            use! responseMsg = _httpClient.SendAsync(requestMsg)

            _logger.Information("Done.")
            return! responseMsg.Content.ReadAsStringAsync()
        }
```

And now you can use all of the [newfangled `IHttpClientFactory` goodness](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2) 
in a (mostly) plain old console app!