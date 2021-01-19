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

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading;

namespace DotnetWebApiBench.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(Environment.ProcessorCount * 4, Environment.ProcessorCount * 4);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            int httpsPort = 5001;
            bool runFromBenchmark = false;
            if (args.Any()) //means it is run from the benchmark
            {
                int.TryParse(args[0], out httpsPort);
                runFromBenchmark = true;
            }

            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .ConfigureKestrel(options =>
                    {
                        if (runFromBenchmark)
                        {
                            options.Listen(IPAddress.Loopback, httpsPort, (listenOptions =>
                            {
                                listenOptions.UseHttps("hostcertificate.pfx", "test");
                            }));
                        }
                        options.Limits.MaxResponseBufferSize = int.MaxValue;
                        options.Limits.MinRequestBodyDataRate = null;
                        options.Limits.MinResponseDataRate = null;
                        options.Limits.MaxRequestBufferSize = int.MaxValue;
                    })
                    .UseStartup<Startup>();
                });

            if (runFromBenchmark)
            {
                hostBuilder.ConfigureLogging(logging => 
                {
                    logging.ClearProviders();
                    logging
                    .AddFile($"Logs/Server_{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}.txt")
                    .AddFilter("Microsoft.*", LogLevel.Error)
                    .AddFilter("System.*", LogLevel.Error);
                });
            }

            return hostBuilder;
        }
    }
}
