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
using DotnetWebApiBench.ApiModel.Product.Request;
using DotnetWebApiBench.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetWebApiBench.DataGenerators
{
    public class TestDataGenerator
    {
        private readonly IProductsClient productsApiClient;
        private readonly ILogger<TestDataGenerator> logger;
        private Random random;

        public TestDataGenerator(IProductsClient productsApiClient, ILogger<TestDataGenerator> logger)
        {
            this.productsApiClient = productsApiClient;
            this.logger = logger;
            this.random = new Random();
        }
        
        public async virtual Task<TimeSpan> GenerateProductsAsync(int numberOfProducts)
        {
            TimeSpan elapsedTime = TimeSpan.Zero;
            using var watcher = new ScopeTimeWatcher((elapsed) =>
            {
                logger.LogInformation($"Test data generated within {elapsed.TotalSeconds} seconds");
                elapsedTime = elapsed;
            });

            for (int i = 1; i <= numberOfProducts; i++)
            {
                int suppliersCount = 100;
                int categoriesCount = 100;

                await this.productsApiClient.AddNewProductAsync(new AddProductRequest() 
                {
                    ProductName = $"Test Product Name for testing {i % 10} {i}",
                    QuantityPerUnit = $"{i} - {i + 10}",
                    SupplierId = (i % suppliersCount) + 1,
                    CategoryId = (i % categoriesCount) + 1,
                    UnitPrice = (decimal)(random.Next(100) + random.NextDouble()),
                    UnitsInStock = random.Next(10),
                    Discontinued = false
                }, CancellationToken.None);
            }

            watcher.Dispose();
            return elapsedTime;
        }
    }
}
