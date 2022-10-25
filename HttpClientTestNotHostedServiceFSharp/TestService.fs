namespace HttpClientTestNotHostedServiceFSharp

open System.Net.Http
open Serilog

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