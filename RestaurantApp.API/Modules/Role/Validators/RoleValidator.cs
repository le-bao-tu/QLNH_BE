using FluentValidation;
using RestaurantApp.API.Modules.Role.DTOs;

namespace RestaurantApp.API.Modules.Role.Validators
{
    public class CreateRoleValidator : AbstractValidator<CreateRoleDto>
    {
        public CreateRoleValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên vai trò không được để trống")
                .MaximumLength(50).WithMessage("Tên vai trò không được vượt quá 50 ký tự");

            RuleFor(x => x.RestaurantId)
                .NotEmpty().WithMessage("ID nhà hàng không được để trống");

            RuleFor(x => x.Permissions)
                .NotEmpty().WithMessage("Ít nhất phải có một quyền được chọn");
        }
    }

    public class UpdateRoleValidator : AbstractValidator<UpdateRoleDto>
    {
        public UpdateRoleValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên vai trò không được để trống")
                .MaximumLength(50).WithMessage("Tên vai trò không được vượt quá 50 ký tự");

            RuleFor(x => x.Permissions)
                .NotEmpty().WithMessage("Ít nhất phải có một quyền được chọn");
        }
    }
}
