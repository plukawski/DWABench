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

using DotnetWebApiBench.DataAccess.Dao;
using DotnetWebApiBench.DataAccess.DataGenerators;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace DotnetWebApiBench.DataAccess.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static DbConnection dbConnection;
        public static void AddNorthwindDataAccess(this IServiceCollection services, 
            string connectionString)
        {
            var options = new DbContextOptionsBuilder<NorthwindDatabaseContext>()
                    .UseSqlite(connectionString)
                    .Options;

            services.AddScoped<NorthwindDatabaseContext>((ctx) => new NorthwindDatabaseContext(options));

            if (connectionString.Contains("memory", System.StringComparison.InvariantCultureIgnoreCase))
            {
                dbConnection = new SqliteConnection(connectionString);
                dbConnection.Open();
                //when we have an in-memory database we must have a main connection which lasts till the end of the process
                //elsewhere the database will be destroyed immediatelly after any other connection ends
                //this is why we never close this connection
            }

            //we must generate critical data in the database (customers, categories, etc.)
            CriticalDataGenerator dataGenerator = new CriticalDataGenerator(new NorthwindDatabaseContext(options));
            dataGenerator.GenerateTestDataAsync().Wait();

            services.AddTransient<IProductDao, ProductDao>();
            services.AddTransient<ICategoryDao, CategoryDao>();
            services.AddTransient<IOrderDao, OrderDao>();
            services.AddTransient<ICustomerDao, CustomerDao>();
        }
    }
}
