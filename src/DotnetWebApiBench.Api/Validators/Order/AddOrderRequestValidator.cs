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

using DotnetWebApiBench.ApiModel.Order.Request;
using FluentValidation;
using System;

namespace DotnetWebApiBench.Api.Validators.Order
{
    public class AddOrderRequestValidator : AbstractValidator<AddOrderRequest>
    {
        public AddOrderRequestValidator()
        {
            this.CascadeMode = CascadeMode.Stop;

            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage($"Field {nameof(AddOrderRequest.CustomerId)} cannot be empty.");

            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage($"Field {nameof(AddOrderRequest.EmployeeId)} cannot be empty.");

            RuleFor(x => x.RequiredDate)
                .NotEmpty().WithMessage($"Field {nameof(AddOrderRequest.RequiredDate)} cannot be empty.")
                .Must(rd => rd > DateTime.Now).WithMessage($"Field {nameof(AddOrderRequest.RequiredDate)} cannot be in the past.")
                ;

            RuleFor(x => x.ShipAddress)
                .NotEmpty().WithMessage($"Field {nameof(AddOrderRequest.ShipAddress)} cannot be empty.");

            RuleFor(x => x.ShipCountry)
                .NotEmpty().WithMessage($"Field {nameof(AddOrderRequest.ShipCountry)} cannot be empty.");

            RuleFor(x => x.ShipName)
                .NotEmpty().WithMessage($"Field {nameof(AddOrderRequest.ShipName)} cannot be empty.");

            RuleFor(x => x.ShipCity)
                .NotEmpty().WithMessage($"Field {nameof(AddOrderRequest.ShipCity)} cannot be empty.");

            RuleFor(x => x.ShipPostalCode)
                .NotEmpty().WithMessage($"Field {nameof(AddOrderRequest.ShipPostalCode)} cannot be empty.");

            RuleFor(x => x.OrderItems)
                .NotEmpty().WithMessage($"Collection {nameof(AddOrderRequest.OrderItems)} cannot be empty.");
        }
    }
}
