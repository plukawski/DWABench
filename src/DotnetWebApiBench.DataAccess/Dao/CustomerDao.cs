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

using DotnetWebApiBench.DataAccess.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetWebApiBench.ApiModel.Customer;

namespace DotnetWebApiBench.DataAccess.Dao
{
    public class CustomerDao : ICustomerDao
    {
        private readonly NorthwindDatabaseContext context;

        public CustomerDao(NorthwindDatabaseContext context)
        {
            this.context = context;
        }

        public async Task<List<CustomerInfo>> GetCustomersAsync(string companyName = null,
            string contactName = null)
        {
            var query = from c in context.Customers
                            .ConditionalWhere(() => !string.IsNullOrWhiteSpace(contactName), x => x.ContactName.Contains(contactName))
                            .ConditionalWhere(() => !string.IsNullOrWhiteSpace(companyName), x => x.CompanyName.Contains(companyName))
                        select new CustomerInfo()
                        {
                            Id = c.Id,
                            CompanyName = c.CompanyName,
                            ContactName = c.ContactName,
                            Country = c.Country,
                        };

            return await query.ToListAsync();
        }
    }
}
