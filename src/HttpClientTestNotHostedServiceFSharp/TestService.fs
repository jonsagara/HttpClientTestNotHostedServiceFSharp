namespace HttpClientTestNotHostedServiceFSharp

open System.Net.Http
open Serilog

type TestService (httpClientFactory : IHttpClientFactory) =

    static let _logger = Log.Logger.ForContext<TestService>()

    let _httpClientFactory = httpClientFactory

    member this.GetMicrosoftAsync() =
        task {
            _logger.Information("Making HTTP request...")

            use httpClient = _httpClientFactory.CreateClient()
            
            use requestMsg = new HttpRequestMessage(HttpMethod.Get, "https://www.microsoft.com")
            requestMsg.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36")

            use! responseMsg = httpClient.SendAsync(requestMsg)

            _logger.Information("Done.")

            return! responseMsg.Content.ReadAsStringAsync()
        }