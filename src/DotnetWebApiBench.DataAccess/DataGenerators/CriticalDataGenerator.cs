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

using DotnetWebApiBench.DataAccess.Entity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetWebApiBench.DataAccess.DataGenerators
{
    public class CriticalDataGenerator
    {
        private readonly NorthwindDatabaseContext context;

        public CriticalDataGenerator(NorthwindDatabaseContext context)
        {
            this.context = context;
            this.context.Database.EnsureCreated();
        }

        public async Task GenerateTestDataAsync()
        {
            if (!context.Categories.Any())
            {
                await this.GenerateCategoriesAsync();
                await this.GenerateEmployeesAsync();
                await this.GenerateSuppliersAsync();
                await this.GenerateCustomersAsync();
            }
        }

        protected async virtual Task GenerateCategoriesAsync()
        {
            context.Categories.Add(new Category() { CategoryName = "Beverages", Description = "Soft drinks, coffees, teas, beers, and ales" });
            context.Categories.Add(new Category() { CategoryName = "Condiments", Description = "Sweet and savory sauces, relishes, spreads, and seasonings" });
            context.Categories.Add(new Category() { CategoryName = "Confections", Description = "Desserts, candies, and sweet breads" });
            context.Categories.Add(new Category() { CategoryName = "Dairy Products", Description = "Cheeses" });
            context.Categories.Add(new Category() { CategoryName = "Grains/Cereals", Description = "Breads, crackers, pasta, and cereal" });
            context.Categories.Add(new Category() { CategoryName = "Meat/Poultry", Description = "Prepared meats" });
            context.Categories.Add(new Category() { CategoryName = "Produce", Description = "Dried fruit and bean curd" });
            context.Categories.Add(new Category() { CategoryName = "Seafood", Description = "Seaweed and fish" });
            await context.SaveChangesAsync();
        }

        protected async virtual Task GenerateEmployeesAsync(int numberOfEmployees = 100)
        {
            for (int i = 1; i <= numberOfEmployees; i++)
            {
                context.Employees.Add(new Employee()
                {
                    FirstName = $"Test First Name {i}",
                    LastName = $"Test Last Name {i}",
                    Title = $"Test title {i}",
                    TitleOfCourtesy = $"Dr.",
                    BirthDate = (new DateTime(2000, 1, 1)).AddDays(i),
                    HireDate = (new DateTime(2019, 1, 1)).AddDays(i),
                    Address = $"Test address line {i}",
                    City = $"Test city {i}",
                    Region = $"Test region {i}",
                    PostalCode = (12345 + i).ToString(),
                    Country = "Neverland",
                    HomePhone = "111 222 333",
                    Extension = (1234 + i).ToString(),
                    Notes = $"Some test not very long notes for employee {i}",
                    PhotoPath = "https://localhost/employee.png"
                });
            }
            await context.SaveChangesAsync();
        }

        protected async virtual Task GenerateSuppliersAsync(int numberOfSuppliers = 100)
        {
            for (int i = 1; i <= numberOfSuppliers; i++)
            {
                context.Suppliers.Add(new Supplier()
                {
                    CompanyName = $"Test Company Name {i}",
                    ContactName = $"Test Contact Name {i}",
                    ContactTitle = $"Test contact title {i}",
                    Address = $"Test address line {i}",
                    City = $"Test city {i}",
                    Region = $"Test region {i}",
                    PostalCode = (12345 + i).ToString(),
                    Country = "Neverland",
                    Phone = "111 222 333",
                    Fax = "222 333 444",
                    HomePage = "https://localhost/home"
                });
            }
            await context.SaveChangesAsync();
        }

        protected async virtual Task GenerateCustomersAsync(int numberOfCustomers = 100)
        {
            for (int i = 1; i <= numberOfCustomers; i++)
            {
                context.Customers.Add(new Customer()
                {
                    Id = $"TEST{i}",
                    CompanyName = $"Test Company Name {i}",
                    ContactName = $"Test Contact Name {i}",
                    ContactTitle = $"Test contact title {i}",
                    Address = $"Test address line {i}",
                    City = $"Test city {i}",
                    Region = $"Test region {i}",
                    PostalCode = (12345 + i).ToString(),
                    Country = "Neverland",
                    Phone = "111 222 333",
                    Fax = "222 333 444",
                });
            }
            await context.SaveChangesAsync();
        }
    }
}
