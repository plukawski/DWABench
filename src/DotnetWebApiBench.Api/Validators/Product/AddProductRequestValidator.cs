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

using DotnetWebApiBench.ApiModel.Product.Request;
using FluentValidation;

namespace DotnetWebApiBench.Api.Validators.Product
{
    public class AddProductRequestValidator : AbstractValidator<AddProductRequest>
    {
        public AddProductRequestValidator()
        {
            this.CascadeMode = CascadeMode.Stop;

            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage($"Field {nameof(AddProductRequest.ProductName)} cannot be empty.")
                .MaximumLength(8000).WithMessage($"Field {nameof(AddProductRequest.ProductName)} is too long.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage($"Field {nameof(AddProductRequest.CategoryId)} cannot be empty.");

            RuleFor(x => x.SupplierId)
                .NotEmpty().WithMessage($"Field {nameof(AddProductRequest.SupplierId)} cannot be empty.");
        }
    }
}
