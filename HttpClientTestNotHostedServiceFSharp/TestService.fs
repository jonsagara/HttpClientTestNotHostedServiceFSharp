module TestService

    open System.Net.Http

    type TestService (httpClientFactory : IHttpClientFactory) =

        let _httpClient = httpClientFactory.CreateClient()

        member this.GetMicrosoftAsync() =
            task {
                use requestMsg = new HttpRequestMessage(HttpMethod.Get, "https://www.microsoft.com")
                use! responseMsg = _httpClient.SendAsync(requestMsg)
                return! responseMsg.Content.ReadAsStringAsync()
            }