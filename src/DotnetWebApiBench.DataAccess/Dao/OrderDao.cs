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
using DotnetWebApiBench.ApiModel.Order;
using DotnetWebApiBench.ApiModel.Order.Request;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetWebApiBench.DataAccess.Dao
{
    public class OrderDao : IOrderDao
    {
        private readonly NorthwindDatabaseContext context;

        public OrderDao(NorthwindDatabaseContext context)
        {
            this.context = context;
        }

        public async Task<List<OrderInfo>> GetOrdersAsync(bool onlyShipped = false, 
            string employeeName = null,
            string customerCompanyName = null,
            DateTime? minRequireDate = null,
            DateTime? minOrderDate = null,
            DateTime? minShippedDate = null,
            string customerId = null)
        {
            await using var tran = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);
            var ordersQuery = from o in context.Orders
                                  .ConditionalWhere(() => onlyShipped, x => x.ShippedDate.HasValue)
                                  .ConditionalWhere(() => minRequireDate.HasValue, x => x.RequiredDate >= minRequireDate.Value)
                                  .ConditionalWhere(() => minOrderDate.HasValue, x => x.OrderDate >= minOrderDate.Value)
                                  .ConditionalWhere(() => minShippedDate.HasValue, x => x.ShippedDate >= minShippedDate.Value)
                              join e in context.Employees
                                  .ConditionalWhere(() => !string.IsNullOrWhiteSpace(employeeName), 
                                        e => ((e.FirstName ?? string.Empty) + " " + (e.LastName ?? string.Empty)).Contains(employeeName))
                                on o.EmployeeId equals e.Id
                              join c in context.Customers
                                  .ConditionalWhere(() => !string.IsNullOrWhiteSpace(customerCompanyName), x => x.CompanyName.Contains(customerCompanyName))
                                  .ConditionalWhere(() => !string.IsNullOrWhiteSpace(customerId), x => x.Id == customerId)
                                on o.CustomerId equals c.Id
                              select new OrderInfo()
                        {
                            Id = o.Id,
                            CustomerId = o.CustomerId,
                            CustomerCompany = c.CompanyName,
                            EmployeeName = (e.FirstName ?? string.Empty)+" "+(e.LastName ?? string.Empty),
                            Freight = o.Freight,
                            OrderDate = o.OrderDate,
                            RequiredDate = o.RequiredDate,
                            ShipAddress = o.ShipAddress,
                            ShipCity = o.ShipCity,
                            ShipCountry = o.ShipCountry,
                            ShipName = o.ShipName,
                            ShippedDate = o.ShippedDate,
                            ShipPostalCode = o.ShipPostalCode,
                            ShipRegion = o.ShipRegion,
                            ShipVia = o.ShipVia
                        };

            var ordersList = await ordersQuery.ToListAsync();

            var ordersDetailsQuery = from od in context.OrderDetails
                                     join p in context.Products on od.ProductId equals p.Id
                                     where ordersQuery.Select(x => x.Id).Contains(od.OrderId)
                                     select new
                                     {
                                         Id = od.Id,
                                         ProductName = p.ProductName,
                                         Quantity = od.Quantity,
                                         UnitPrice = od.UnitPrice,
                                         Discount = od.Discount,
                                         od.OrderId
                                     };

            var orderDetailsList = await ordersDetailsQuery.ToListAsync();

            ordersList.ForEach(order => 
            {
                order.OrderItems = orderDetailsList.Where(x => x.OrderId == order.Id)
                .Select(x => new OrderItemInfo()
                {
                    Id = x.Id,
                    ProductName = x.ProductName,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    Discount = x.Discount,
                }).ToList();
            });

            return ordersList;
        }

        public async Task<long> AddNewOrderAsync(AddOrderRequest addOrderRequest)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            var unitPrices = (await context.Products
                    .Where(x => addOrderRequest.OrderItems.Select(oi => oi.ProductId).Contains(x.Id))
                    .Select(x => new
                    {
                        x.UnitPrice,
                        x.Id
                    })
                    .ToListAsync());

            var dbOrder = new Order();
            dbOrder.CustomerId = addOrderRequest.CustomerId;
            dbOrder.EmployeeId = addOrderRequest.EmployeeId;
            dbOrder.OrderDate = DateTime.Now.Date;
            dbOrder.RequiredDate = addOrderRequest.RequiredDate.Date;
            dbOrder.ShipAddress = addOrderRequest.ShipAddress;
            dbOrder.ShipCity = addOrderRequest.ShipCity;
            dbOrder.ShipCountry = addOrderRequest.ShipCountry;
            dbOrder.ShipName = addOrderRequest.ShipName;
            dbOrder.ShipPostalCode = addOrderRequest.ShipPostalCode;

            context.Orders.Add(dbOrder);
            await context.SaveChangesAsync();

            foreach (var item in addOrderRequest.OrderItems)
            {
                decimal unitPrice = unitPrices
                    .Where(x => x.Id == item.ProductId)
                    .Select(x => x.UnitPrice)
                    .SingleOrDefault();

                OrderDetail dbOrderDetail = new OrderDetail();
                dbOrderDetail.OrderId = dbOrder.Id;
                dbOrderDetail.ProductId = item.ProductId;
                dbOrderDetail.Id = $"{dbOrder.Id}/{item.ProductId}";
                dbOrderDetail.UnitPrice = unitPrice;
                dbOrderDetail.Quantity = item.Quantity;
                dbOrderDetail.Discount = 0;

                context.OrderDetails.Add(dbOrderDetail);
            }
            await context.SaveChangesAsync();

            await transaction.CommitAsync();
            return dbOrder.Id;
        }
    }
}
