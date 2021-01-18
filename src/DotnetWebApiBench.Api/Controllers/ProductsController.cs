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
using DotnetWebApiBench.ApiModel.Product;
using DotnetWebApiBench.ApiModel.Product.Request;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace DotnetWebApiBench.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : BaseController
    {
        private readonly IProductDao productDao;
        private readonly IMemoryCache memoryCache;

        public ProductsController(IProductDao productDao, IMemoryCache memoryCache)
        {
            this.productDao = productDao;
            this.memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<List<ProductInfo>> GetProductsAsync(string productName = null, 
            bool onlyInStock = false)
        {
            string cacheKey = $"{nameof(GetProductsAsync)}_{productName}_{onlyInStock}";

            List<ProductInfo> products = memoryCache.Get<List<ProductInfo>>(cacheKey);
            if (products == null)
            {
                products = await this.productDao.GetProductsAsync(productName, onlyInStock);
                memoryCache.Set(cacheKey, products, new MemoryCacheEntryOptions() 
                { 
                    AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(300)
                });
            }
                
            return products;
        }

        [HttpPost]
        public async Task AddNewProductAsyns([FromBody] AddProductRequest request)
        {
            await this.productDao.AddNewProductAsync(request);
        }
    }
}
