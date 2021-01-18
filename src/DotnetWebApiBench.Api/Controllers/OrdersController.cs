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
using DotnetWebApiBench.ApiModel.Order;
using DotnetWebApiBench.ApiModel.Order.Request;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace DotnetWebApiBench.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : BaseController
    {
        private readonly IOrderDao orderDao;
        private readonly IMemoryCache memoryCache;

        public OrdersController(IOrderDao orderDao, IMemoryCache memoryCache)
        {
            this.orderDao = orderDao;
            this.memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<List<OrderInfo>> GetOrdersAsync(bool onlyShipped = false,
            string employeeName = null,
            string customerCompanyName = null,
            DateTime? minRequireDate = null,
            DateTime? minOrderDate = null,
            DateTime? minShippedDate = null,
            string customerId = null)
        {
            string cacheKey = $"{nameof(GetOrdersAsync)}_{customerId}";

            List<OrderInfo> orders = memoryCache.Get<List<OrderInfo>>(cacheKey);

            if (orders == null)
            {
                orders = await this.orderDao.GetOrdersAsync(onlyShipped, employeeName, customerCompanyName,
                   minRequireDate, minOrderDate, minShippedDate, customerId);
                memoryCache.Set(cacheKey, orders, new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(300)
                });
            }

            return orders;
        }

        [HttpPost]
        public async Task<long> AddNewOrderAsync(AddOrderRequest addOrderRequest)
        {
            string getOrdersCacheKey = $"{nameof(GetOrdersAsync)}_{addOrderRequest.CustomerId}";
            memoryCache.Remove(getOrdersCacheKey);  //we remove the cache after we add a new order
            //the above logic is simplified to the case which is used currently by benchmark. 
            //In real life we should remove all possible keys created for the customer and variations of parameters passed in the Get endpoint

            return await this.orderDao.AddNewOrderAsync(addOrderRequest);
        }
    }
}
