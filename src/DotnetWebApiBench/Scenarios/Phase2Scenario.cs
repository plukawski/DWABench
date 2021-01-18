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

using DotnetWebApiBench.ApiClient.Interfaces;
using DotnetWebApiBench.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetWebApiBench.Scenarios
{
    public class Phase2Scenario
    {
        private readonly IProductsClient productsClient;
        private readonly IOrdersClient ordersClient;
        private readonly ICustomersClient customersClient;
        private readonly ILogger<Phase2Scenario> logger;
        private int totalRequests = 0;
        public bool ErrorsOccured { get; private set; }

        public Phase2Scenario(IProductsClient productsClient,
            IOrdersClient ordersClient,
            ICustomersClient customersClient,
            ILogger<Phase2Scenario> logger
            )
        {
            this.productsClient = productsClient;
            this.ordersClient = ordersClient;
            this.customersClient = customersClient;
            this.logger = logger;
        }

        public async Task<int> ExecuteScenarioAsync(int numberOfSecondsToRun, int numberOfConcurrentUsers)
        {
            CancellationTokenSource phase2CancellationTokenSource = new CancellationTokenSource();
            phase2CancellationTokenSource.CancelAfter(numberOfSecondsToRun * 1000);
            List<Task> tasks = new List<Task>();

            logger.LogInformation($"Phase 2: Executing end user scenario simulating {numberOfConcurrentUsers} concurrent users.");

            var watcher = new ScopeTimeWatcher((elapsed) =>
            {
                logger.LogInformation($"Phase 2 execution time: {elapsed.TotalSeconds} seconds");
            });

            for (int i = 0; i < numberOfConcurrentUsers; i++)
            {
                tasks.Add(DoSingleUserWorkAsync(phase2CancellationTokenSource.Token));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                ErrorsOccured = true;
                logger.LogError($"An error interrupted phase 2 execution: {ex.Message}");
            }

            watcher.Dispose();
            logger.LogInformation($"Total requests processed: {totalRequests}");
            logger.LogInformation($"Requests per second: {totalRequests / numberOfSecondsToRun}");
            return totalRequests;
        }

        protected virtual async Task DoSingleUserWorkAsync(CancellationToken cancellationToken)
        {
            int iteration = 0;
            var random = new Random();
            var customers = await customersClient.GetCustomersAsync();
            var customer = customers.ElementAt(random.Next(1, customers.Count()));
            Interlocked.Increment(ref totalRequests);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var products5 = await productsClient.GetProductsAsync("for testing 5", true, cancellationToken);
                    var products4 = await productsClient.GetProductsAsync("for testing 4", true, cancellationToken);
                    var products6 = await productsClient.GetProductsAsync("for testing 6", true, cancellationToken);

                    var orderItems = products4.Take(1)
                        .Concat(products5.Take(1))
                        .Concat(products6.Take(1))
                        .Select(x => new ApiModel.Order.NewOrderItemInfo()
                        {
                            ProductId = x.Id,
                            Quantity = 4
                        })
                        .ToList();

                    bool shouldAddOrder = (iteration++) % 4 == 0;

                    if (shouldAddOrder)
                    {
                        await ordersClient.AddNewOrderAsync(new ApiModel.Order.Request.AddOrderRequest()
                        {
                            CustomerId = customer.Id,
                            EmployeeId = 2,
                            Freight = 1,
                            RequiredDate = DateTime.Now.AddDays(3),
                            ShipAddress = "Test address",
                            ShipCity = "Never",
                            ShipCountry = "Neverland",
                            ShipName = "Test ship name",
                            ShipPostalCode = "12345",
                            ShipRegion = "Test region",
                            OrderItems = orderItems
                        }, cancellationToken);
                        Interlocked.Increment(ref totalRequests);
                    }

                    var orders = await ordersClient.GetOrdersAsync(customerId: customer.Id,
                        cancellationToken: cancellationToken);

                    Interlocked.Increment(ref totalRequests);
                    Interlocked.Increment(ref totalRequests);
                    Interlocked.Increment(ref totalRequests);
                    Interlocked.Increment(ref totalRequests);
                }
                catch (Exception ex) when (ex is IOException || ex is TaskCanceledException)
                {
                    ;
                }
            }
        }
    }
}
