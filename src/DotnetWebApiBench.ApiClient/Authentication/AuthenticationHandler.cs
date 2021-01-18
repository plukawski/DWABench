/*
Copyright(c) 2020-2021 Przemysław Łukawski

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetWebApiBench.ApiClient.Authentication
{
    class AuthenticationHandler : DelegatingHandler
    {
        private readonly string baseUrl;
        private readonly HttpMessageHandler sendingHandler;
        private static string token;
        static SemaphoreSlim tokenCriticalSection = new SemaphoreSlim(1, 1);

        public AuthenticationHandler(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        public AuthenticationHandler(HttpMessageHandler sendingHandler)
        {
            this.baseUrl = "https://localhost";
            this.sendingHandler = sendingHandler;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                token = await this.GetAccessTokenAsync();
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                token = await this.GetAccessTokenAsync();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                response = await base.SendAsync(request, cancellationToken);
            }

            return response;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            await tokenCriticalSection.WaitAsync();
            try
            {
                var noSslCheckHandler = new HttpClientHandler();
                noSslCheckHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                noSslCheckHandler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };

                using var httpClient = this.sendingHandler != null
                    ? new HttpClient(this.sendingHandler)
                    : new HttpClient(noSslCheckHandler);
                httpClient.BaseAddress = new Uri(this.baseUrl);

                var disco = await httpClient.GetDiscoveryDocumentAsync();
                if (disco.IsError) throw new Exception(disco.Error);

                var response = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = "test",
                    ClientSecret = "test",
                });

                if (response.IsError) throw new Exception(response.Error);
                return response.AccessToken;
            }
            finally 
            {
                tokenCriticalSection.Release();
            }
        }
    }
}
