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

using DotnetWebApiBench.ApiModel.Order;
using DotnetWebApiBench.ApiModel.Order.Request;
using Refit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetWebApiBench.ApiClient.Interfaces
{
    public interface IOrdersClient
    {
        [Get(@"/orders?onlyShipped={onlyShipped}
                &employeeName={employeeName}
                &customerCompanyName={customerCompanyName}
                &minRequireDate={minRequireDate}
                &minOrderDate={minOrderDate}
                &minShippedDate={minShippedDate}
                &customerId={customerId}")]
        public Task<List<OrderInfo>> GetOrdersAsync(CancellationToken cancellationToken,
            bool onlyShipped = false,
            string employeeName = null,
            string customerCompanyName = null,
            DateTime? minRequireDate = null,
            DateTime? minOrderDate = null,
            DateTime? minShippedDate = null,
            string customerId = null);

        [Post("/orders")]
        public Task<long> AddNewOrderAsync(AddOrderRequest addOrderRequest, CancellationToken cancellationToken);
    }
}
