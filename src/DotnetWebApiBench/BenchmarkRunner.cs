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

using CsvHelper;
using DotnetWebApiBench.ApiClient.Interfaces;
using DotnetWebApiBench.Helpers;
using DotnetWebApiBench.Models;
using DotnetWebApiBench.Scenarios;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetWebApiBench
{
    internal class BenchmarkRunner : IDisposable
    {
        public const string IN_MEMORY_DATABASE_CONN_STRING = "Data Source=NorthwindInMemory;Mode=Memory;Cache=Shared";
        private const int DEFAULT_PHASE1_RECORDS = 10000;
        private const int DEFAULT_PHASE2_SECONDS = 40;

        private readonly ILogger<BenchmarkRunner> logger;
        private readonly Phase1Scenario phase1;
        private readonly Phase2Scenario phase2;
        private readonly IHeartbeatClient heartbeatClient;
        private readonly IConfiguration configuration;
        private Process server;
        private DateTime executionTime = DateTime.UtcNow;

        public BenchmarkRunner(ILogger<BenchmarkRunner> logger,
            Phase1Scenario phase1,
            Phase2Scenario phase2,
            IHeartbeatClient heartbeatClient,
            IConfiguration configuration)
        {
            ;
            this.logger = logger;
            this.phase1 = phase1;
            this.phase2 = phase2;
            this.heartbeatClient = heartbeatClient;
            this.configuration = configuration;
        }

        public async Task RunBenchmarkAsync(string hostHttpsUrl, int httpsPort, DateTime executionTime)
        {
            this.executionTime = executionTime;
            
            PrintHardwareInformation();
            string connectionString = PrepareTestDatabaseConnectionString();

            logger.LogDebug($"Starting internal Web server using address '{hostHttpsUrl}' ...");
            this.server = StartWebApiServer(httpsPort, connectionString);

            bool serverReady = await WaitForServerToBeReadyAsync();
            if (!serverReady)
            {
                logger.LogError("Unable to access the server within 120 seconds. Exiting...");
                this.server.Kill(true);
                KillWebApiServers();
                return;
            }

            int.TryParse(configuration["phase1-records"], out int recordsToInsert);
            recordsToInsert = recordsToInsert <= 0 ? DEFAULT_PHASE1_RECORDS : recordsToInsert;

            TimeSpan elapsedTime = await phase1.ExecuteScenarioAsync(recordsToInsert);

            int.TryParse(configuration["phase2-users"], out int numberOfConcurrentUsers);
            int.TryParse(configuration["phase2-seconds"], out int secondsToRun);
            secondsToRun = secondsToRun <= 0 ? DEFAULT_PHASE2_SECONDS : secondsToRun;
            numberOfConcurrentUsers = numberOfConcurrentUsers <= 0
                ? Environment.ProcessorCount - 1   //by default we leave one CPU thread for other tasks - this increases the performance
                : numberOfConcurrentUsers;

            int totalRequests = await phase2.ExecuteScenarioAsync(secondsToRun, numberOfConcurrentUsers);

            BenchmarkResults testResults = new BenchmarkResults()
            {
                ExecutionTimeUtc = executionTime.ToString("yyyy-MM-dd HH:mm:ss"),
                Phase1Time = (decimal)elapsedTime.TotalSeconds,
                Phase2Time = secondsToRun,
                Phase2TotalRequests = totalRequests,
                Phase2RequestsPerSecond = totalRequests / secondsToRun,
                ErrorsOccured = phase1.ErrorsOccured || phase2.ErrorsOccured
            };

            WriteResultsToCsvFile(testResults);
        }

        public void Dispose()
        {
            this.server?.Kill(true);
            KillWebApiServers();
        }

        #region internals

        internal static void KillWebApiServers()
        {
            Process[] processesToKill = Process.GetProcessesByName("DotnetWebApiBench.Api");
            foreach (var process in processesToKill)
            {
                process.Kill(true);
            }
        }

        #endregion

        #region privates

        private void PrintHardwareInformation()
        {
            try
            {
                logger.LogInformation(string.Empty);
                logger.LogInformation("--------------Hardware information----------------");
                logger.LogInformation("CPU Information:");
                var processorNames = HardwareInformationHelper.GetProcessorName();
                for (int i = 0; i < processorNames.Count; i++)
                {
                    logger.LogInformation($"{i + 1}. {processorNames[i]}");
                }
                logger.LogInformation($"Number of total available logical cores: {Environment.ProcessorCount}");

                logger.LogInformation("\r\nRAM Information:");
                logger.LogInformation($"Total available memory:{GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024} MB");

                logger.LogInformation("\r\nDisk Information:");
                logger.LogInformation(HardwareInformationHelper.GetDiskModel(Environment.CurrentDirectory.First()));
                logger.LogInformation("--------------Hardware information----------------");
                logger.LogInformation(string.Empty);
            }
            catch (Exception ex)
            {
                logger.LogError($"Unable to obtain full hardware information: {ex.Message}");
            }
        }

        private string PrepareTestDatabaseConnectionString()
        {
            string dbFilePath = "./DotnetWebApiBench_test.db";
            if (File.Exists(dbFilePath))
            {
                File.Delete(dbFilePath);
            }
            string connectionString = "";
            
            if (configuration["db-type"] != null && configuration["db-type"].Equals("SQLServer", StringComparison.InvariantCultureIgnoreCase))
            {
                connectionString = $"Data Source={configuration["DBAdress"]};initial catalog={configuration["DBName"]};user id={configuration["DBUserName"]};password={configuration["DBPassword"]};";
            }
            else 
            { 
                connectionString = $"Data Source={dbFilePath};Cache=Shared";
                if (configuration["memory"]?.Equals("true", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    connectionString = IN_MEMORY_DATABASE_CONN_STRING;
                }
            }
            logger.LogInformation($"Database connection string: {connectionString}");
            return connectionString;
        }

        private async Task<bool> WaitForServerToBeReadyAsync()
        {
            DateTime startTime = DateTime.Now;
            logger.LogDebug("Waiting for server to be ready...");
            while ((DateTime.Now - startTime).TotalSeconds < 120)
            {
                try
                {
                    await heartbeatClient.HeartbeatAsync();
                    return true;
                }
                catch (Exception)
                {
                    //nothing to do here
                }
            }

            return false;
        }

        private Process StartWebApiServer(int httpsPort, string connectionString)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "DotnetWebApiBench.Api.exe";
            if (configuration["db-type"] != null && configuration["db-type"].Equals("SQLServer", StringComparison.InvariantCultureIgnoreCase))
            { 
                startInfo.Arguments = $"{httpsPort} /ConnectionStrings:Northwind=\"{connectionString}\" /db-type={configuration["db-type"]}"; 
            }
            else 
            { 
                startInfo.Arguments = $"{httpsPort} /ConnectionStrings:Northwind=\"{connectionString}\""; 
            }
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            return Process.Start(startInfo);
        }

        private void WriteResultsToCsvFile(BenchmarkResults testResults)
        {
            using (var stream = File.Open($"Results/DWABench_results_{executionTime.ToString("yyyyMMdd")}.csv", FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                if (stream.Length > 0)
                {
                    // Don't write the header again.
                    csv.Configuration.HasHeaderRecord = false;
                }

                csv.WriteRecords(new BenchmarkResults[] { testResults });
            }
        }

        #endregion
    }
}
