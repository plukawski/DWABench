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
using DotnetWebApiBench.DataAccess.Extensions;
using DotnetWebApiBench.ApiModel.Product;
using DotnetWebApiBench.ApiModel.Product.Request;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetWebApiBench.DataAccess.Dao
{
    public class ProductDao : IProductDao
    {
        private readonly NorthwindDatabaseContext context;

        public ProductDao(NorthwindDatabaseContext context)
        {
            this.context = context;
        }

        public async Task<List<ProductInfo>> GetProductsAsync(string productName = null, bool onlyInStock = false)
        {
            await using var tran = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);
            var query = from p in context.Products
                            .ConditionalWhere(() => onlyInStock, x => x.UnitsInStock > 0)
                            .ConditionalWhere(() => !string.IsNullOrWhiteSpace(productName), x => x.ProductName.Contains(productName))
                        select new ProductInfo()
                        {
                            Id = p.Id,
                            CategoryId = p.CategoryId,
                            Discontinued = p.Discontinued,
                            ProductName = p.ProductName,
                            UnitPrice = p.UnitPrice,
                            UnitsInStock = p.UnitsInStock,
                            QuantityPerUnit = p.QuantityPerUnit
                        };

            return await query.ToListAsync();
        }

        public async Task AddNewProductAsync(AddProductRequest request)
        {
            var dbProduct = new Product();
            dbProduct.ProductName = request.ProductName;
            dbProduct.UnitsInStock = request.UnitsInStock;
            dbProduct.UnitPrice = request.UnitPrice;
            dbProduct.QuantityPerUnit = request.QuantityPerUnit;
            dbProduct.Discontinued = request.Discontinued;
            dbProduct.CategoryId = request.CategoryId;
            dbProduct.SupplierId = request.SupplierId;

            context.Products.Add(dbProduct);
            await context.SaveChangesAsync();
        }
    }
}
