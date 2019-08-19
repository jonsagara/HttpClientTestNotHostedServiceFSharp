# Use `HttpClient` via `IHttpClientFactory` in a .NET Core 2.2 console app without implementing `IHostedService` #

> If you're looking for the C# version, go [here](https://github.com/jonsagara/HttpClientTestNotHostedService).

Not that there's anything wrong with `IHostedService`, but sometimes you just want a plain old console app without having to implement another interface just so 
that you can inject and use `IHttpClientFactory`.

There is still some ceremony involved with setting up the Generic Host `HostBuilder` so that you can inject `IHttpClientFactory` into your classes,
but beyond that you merely create an `IServiceScope` to make everything work:

```fsharp
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
```

This code creates an `IServiceScope` in `main`, and then uses that to get our registered `TestService` class, which returns the HTML of https://www.microsoft.com
as a string. We then display the first 1,000 characters.

The `TestService` class looks like this:

```fsharp
module TestService

    open System.Net.Http

    type TestService (httpClientFactory : IHttpClientFactory) =

        let _httpClient = httpClientFactory.CreateClient()

        member this.AsyncGetMicrosoft() =
            async {
                use requestMsg = new HttpRequestMessage(HttpMethod.Get, "https://www.microsoft.com")
                use! responseMsg = _httpClient.SendAsync(requestMsg) |> Async.AwaitTask
                let! content = responseMsg.Content.ReadAsStringAsync() |> Async.AwaitTask
                return content
            }
```

And now you can use all of the [newfangled `IHttpClientFactory` goodness](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2) 
in a (mostly) plain old console app!