using FluentValidation;
using RestaurantApp.API.Modules.Customer.Services;

namespace RestaurantApp.API.Modules.Customer.Validators
{
    public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
    {
        public CreateCustomerValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ tên không được để trống")
                .MaximumLength(100).WithMessage("Họ tên không quá 100 ký tự");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Số điện thoại là bắt buộc")
                .Matches(@"^\+?[0-9]{10,12}$").WithMessage("Số điện thoại không hợp lệ");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Email không hợp lệ");

            RuleFor(x => x.RestaurantId).NotEmpty().WithMessage("ID nhà hàng không hợp lệ");
        }
    }
}
