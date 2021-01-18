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

using IdentityServer4.Models;
using System.Collections.Generic;
using System.Linq;

namespace DotnetWebApiBench.Auth.Config
{
    public class Config
    {
        public const string DOTNETWEBAPIBENCH_API = "DotnetWebApiBench.API";

        public static IEnumerable<ApiResource> Apis =>
            new List<ApiResource>
            {
                new ApiResource(DOTNETWEBAPIBENCH_API, DOTNETWEBAPIBENCH_API)
                {
                    Scopes = new [] { DOTNETWEBAPIBENCH_API }
                }
            };

        public static IEnumerable<ApiScope> Scopes =>
            new List<ApiScope>
            {
                new ApiScope(DOTNETWEBAPIBENCH_API, DOTNETWEBAPIBENCH_API)
            };

        //place for hardcoded clients if any required
        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client()
                { 
                    ClientId = "test",
                    ClientSecrets = new []
                    {
                        new Secret()
                        {
                            Value = "test".Sha256()
                        }
                    },
                    AllowedGrantTypes = new string[]
                    {
                        GrantType.ClientCredentials
                    },
                    AllowedScopes = Apis
                        .Select(x => x.Name)
                        .ToArray()
                }
            };
    }
}
