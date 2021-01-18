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

using DotnetWebApiBench.ApiClient.Authentication;
using DotnetWebApiBench.ApiClient.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace DotnetWebApiBench.ApiClient.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDotnetWebApiBenchApiRefitClients(this IServiceCollection services, string apiAddress, bool allowInvalidCerts = false)
        {
            services.AddTransient<AuthenticationHandler>(ctx => new AuthenticationHandler(apiAddress));

            RefitSettings refitSettings = new RefitSettings()
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer()
            };

            List<IHttpClientBuilder> httpClientBuilders = new List<IHttpClientBuilder>();
            httpClientBuilders.Add(services.AddRefitClient<IProductsClient>(refitSettings));
            httpClientBuilders.Add(services.AddRefitClient<IOrdersClient>(refitSettings));
            httpClientBuilders.Add(services.AddRefitClient<ICustomersClient>(refitSettings));
            httpClientBuilders.Add(services.AddRefitClient<IHeartbeatClient>(refitSettings));

            foreach (IHttpClientBuilder clientBuilder in httpClientBuilders)
            {
                clientBuilder.ConfigureHttpClient((client) =>
                {
                    client.BaseAddress = new Uri(apiAddress);
                })
                .AddHttpMessageHandler<AuthenticationHandler>();

                if (allowInvalidCerts)
                {
                    var noSslCheckHandler = new HttpClientHandler();
                    noSslCheckHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    noSslCheckHandler.UseProxy = false;
                    noSslCheckHandler.ServerCertificateCustomValidationCallback =
                        (httpRequestMessage, cert, cetChain, policyErrors) =>
                        {
                            return true;
                        };

                    clientBuilder.ConfigurePrimaryHttpMessageHandler(() => noSslCheckHandler);
                }
            }
        }

        public static void AddDotnetWebApiBenchApiRefitClients(this IServiceCollection services, HttpMessageHandler sendHandler)
        {
            services.AddTransient<AuthenticationHandler>(ctx => new AuthenticationHandler(sendHandler));

            List<IHttpClientBuilder> httpClientBuilders = new List<IHttpClientBuilder>();
            httpClientBuilders.Add(services.AddRefitClient<IProductsClient>());
            httpClientBuilders.Add(services.AddRefitClient<IOrdersClient>());
            httpClientBuilders.Add(services.AddRefitClient<ICustomersClient>());

            foreach (IHttpClientBuilder clientBuilder in httpClientBuilders)
            {
                clientBuilder.ConfigureHttpClient((client) =>
                {
                    client.BaseAddress = new Uri("https://localhost");
                })
                .AddHttpMessageHandler<AuthenticationHandler>()
                .ConfigurePrimaryHttpMessageHandler(() => sendHandler);
            }
        }
    }
}
