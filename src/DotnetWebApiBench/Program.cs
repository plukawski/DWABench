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

using DotnetWebApiBench.ApiClient.Extensions;
using DotnetWebApiBench.CustomFormatters;
using DotnetWebApiBench.DataAccess.Enums;
using DotnetWebApiBench.DataGenerators;
using DotnetWebApiBench.Helpers;
using DotnetWebApiBench.Models.Config;
using DotnetWebApiBench.Scenarios;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DotnetWebApiBench
{
    class Program
    {
        public static IServiceProvider Container;
        public static IConfiguration Configuration;
        private static ILogger<Program> logger;
        private static DateTime executionTime = DateTime.UtcNow;

        public static async Task Main(string[] args)
        {
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            BenchmarkRunner.KillWebApiServers();    //ensure no previously launched API servers are running
            ParseParameters(args);
            int httpsPort = HostUrlGenerator.GetFreeRandomPort();
            string hostHttpsUrl = $"https://localhost:{httpsPort}";
            ConfigureServices(hostHttpsUrl);

            WriteInformationLine($"DWABench v{version.Major}.{version.Minor} - .NET Web API benchmarking tool.");
            Console.WriteLine("Created by Przemysław Łukawski.");

            if (args.Any(x => x.Equals("/?", StringComparison.InvariantCultureIgnoreCase)
                || x.Equals("--help", StringComparison.InvariantCultureIgnoreCase)))
            {
                PrintUsage();
                return;
            }

            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            Console.CancelKeyPress += Console_CancelKeyPress;

            using (var mainScope = Container.CreateScope())
            {
                var runner = mainScope.ServiceProvider.GetRequiredService<BenchmarkRunner>();
                await runner.RunBenchmarkAsync(hostHttpsUrl, httpsPort, executionTime);
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static void ParseParameters(string[] args)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddCommandLine(args);
            Configuration = configurationBuilder.Build();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            BenchmarkRunner.KillWebApiServers();
            Environment.Exit(1);
        }

        private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            BenchmarkRunner.KillWebApiServers();
        }

        private static void ConfigureServices(string hostHttpsUrl)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddLogging(logger => 
            {
                logger
                    .AddConsole(options => options.FormatterName = nameof(PlainTextConsoleFormatter))
                    .AddConsoleFormatter<PlainTextConsoleFormatter, ConsoleFormatterOptions>()
                    .AddFile($"Results/DWABench_execution_{executionTime.ToString("yyyyMMdd_HHmmss")}.txt", options => 
                    {
                        options.FormatLogEntry = (msg) =>
                        {
                            if (msg.LogLevel >= LogLevel.Information)   //in execution results file we only want to have information and higher levels
                            {
                                return msg.Message;
                            }
                            return string.Empty;
                        };
                    })
                    .AddFile($"Logs/{executionTime.ToString("yyyyMMdd_HHmmss")}.txt")
                    .AddFilter("Microsoft.*", LogLevel.Error)
                    .AddFilter("System.*", LogLevel.Error);
            });
            services.AddDotnetWebApiBenchApiRefitClients(hostHttpsUrl, true);
            services.AddSingleton<TestDataGenerator>();
            services.AddSingleton<Phase1Scenario>();
            services.AddSingleton<Phase2Scenario>();
            services.AddSingleton<PlainTextConsoleFormatter>();
            services.AddScoped<BenchmarkRunner>();
            services.AddSingleton<IConfiguration>(ctx => Configuration);

            services.Configure<BenchmarkSettings>(options => 
            {
                options.Phase1Rescords = Configuration.GetValue<int>("phase1-records", 10000);
                options.Phase2Seconds = Configuration.GetValue<int>("phase2-seconds", 40);
                options.Phase2Users = Configuration.GetValue<int>("phase2-users", Environment.ProcessorCount - 1);
                options.UseMemoryDatabase = Configuration.GetValue<bool>("memory", false);
                options.DatabaseType = Enum.Parse<DbTypeEnum>(Configuration.GetValue<string>("db-type", DbTypeEnum.SQLite.ToString()));
                options.DbName = Configuration.GetValue<string>("db-name", "DWABench_Northwind");
                options.DbServer = Configuration.GetValue<string>("db-address", "localhost");
                options.DbUserName = Configuration.GetValue<string>("db-username", null);
                options.DbPassword = Configuration.GetValue<string>("db-password", null);
                options.DbConnectionString = Configuration.GetValue<string>("db-connectionstring", null);
            });

            Container = services.BuildServiceProvider();
            logger = Container.GetRequiredService<ILogger<Program>>();
        }

        private static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine("DotnetWebApiBench.exe [/memory=false] [/phase1-records=10000] [/phase2-seconds=20] [/phase2-users=7] [/db-type=SQLite]");
            Console.WriteLine();
            Console.WriteLine("memory              When set to true will use database located in memory instead of disk. Defaults to false.");
            Console.WriteLine("phase1-records      Specifies number of records to generate in phase 1. Defaults to 10000");
            Console.WriteLine("phase2-seconds      Specifies number of seconds phase 2 should take. Defaults to 20");
            Console.WriteLine("phase2-users        Specifies number of concurrent users in phase 2. Defaults to logical cores minus one");
            Console.WriteLine("db-type             Specifies type of the database (SQLServer or SQLite). Defaults to SQLite");
            Console.WriteLine("db-connectionstring Specifies full connection string to the database.");
            Console.WriteLine("db-name             Specifies database name for non SQLite database. Ignored when full connection string is provided.");
            Console.WriteLine("db-address          Specifies database server address for non SQLite database. Ignored when full connection string is provided.");
            Console.WriteLine("db-username         Specifies database user name for non SQLite database. Ignored when full connection string is provided.");
            Console.WriteLine("db-password         Specifies database user password for non SQLite database. Ignored when full connection string is provided.");
            Console.WriteLine("");
        }

        private static void WriteInformationLine(string message = null)
        {
            message ??= string.Empty;
            if (logger == null)
            {
                Console.WriteLine(message);
            }
            
            logger?.LogInformation(message);
        }
    }
}
