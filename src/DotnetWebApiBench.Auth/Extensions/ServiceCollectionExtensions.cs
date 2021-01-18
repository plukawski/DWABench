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

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DotnetWebApiBench.Auth.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDotnetWebApiBenchAuthentication(this IServiceCollection services)
        {
            var builder = services.AddIdentityServer()
                            .AddInMemoryApiScopes(Config.Config.Scopes)
                            .AddInMemoryApiResources(Config.Config.Apis)
                            .AddInMemoryClients(Config.Config.Clients);

            SecurityKey signingKey = null;

            var assembly = Assembly.GetExecutingAssembly();
            using Stream stream = assembly.GetManifestResourceStream($"{nameof(DotnetWebApiBench)}.{nameof(DotnetWebApiBench.Auth)}.tempkey.jwk");
            using StreamReader reader = new StreamReader(stream);

            string json = reader.ReadToEnd();
            JsonWebKey jsonWebKey = JsonWebKey.Create(json);
            SigningCredentials credential = new SigningCredentials(jsonWebKey, jsonWebKey.Alg);

            builder.AddSigningCredential(credential);
            signingKey = jsonWebKey;

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = false,
                    ValidateAudience = true,
                    ValidAudiences = Config.Config.Apis.Select(x => x.Name),
                };

                x.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = context =>
                    {
                        return Task.CompletedTask;
                    }
                };
            });
        }
    }
}
