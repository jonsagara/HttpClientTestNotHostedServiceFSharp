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