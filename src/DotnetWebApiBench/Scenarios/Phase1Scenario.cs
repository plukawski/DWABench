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

using DotnetWebApiBench.DataGenerators;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DotnetWebApiBench.Scenarios
{
    public class Phase1Scenario
    {
        private readonly TestDataGenerator testDataGenerator;
        private readonly ILogger<Phase1Scenario> logger;
        public bool ErrorsOccured { get; private set; }

        public Phase1Scenario(TestDataGenerator testDataGenerator, ILogger<Phase1Scenario> logger)
        {
            this.testDataGenerator = testDataGenerator;
            this.logger = logger;
        }

        public async Task<TimeSpan> ExecuteScenarioAsync(int numberOfRecords = 10000)
        {
            logger.LogInformation("Phase 1: Generating test data...");
            try
            {
                return await testDataGenerator.GenerateProductsAsync(numberOfRecords);
            }
            catch (Exception ex)
            {
                ErrorsOccured = true;
                logger.LogError($"An error interrupted phase 1 execution: {ex.Message}");
                throw;
            }
        }
    }
}
