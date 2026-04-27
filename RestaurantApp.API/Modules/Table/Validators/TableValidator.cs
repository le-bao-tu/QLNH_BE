using FluentValidation;
using RestaurantApp.API.Modules.Table.DTOs;

namespace RestaurantApp.API.Modules.Table.Validators
{
    public class CreateTableValidator : AbstractValidator<CreateTableDto>
    {
        public CreateTableValidator()
        {
            RuleFor(x => x.BranchId).NotEmpty().WithMessage("Vui lòng chọn chi nhánh");
            RuleFor(x => x.TableNumber).GreaterThan(0).WithMessage("Số bàn phải lớn hơn 0");
            RuleFor(x => x.Capacity).GreaterThan(0).WithMessage("Sức chứa phải lớn hơn 0");
        }
    }

    public class UpdateTableValidator : AbstractValidator<UpdateTableDto>
    {
        public UpdateTableValidator()
        {
            RuleFor(x => x.TableNumber).GreaterThan(0).When(x => x.TableNumber.HasValue).WithMessage("Số bàn phải lớn hơn 0");
            RuleFor(x => x.Capacity).GreaterThan(0).When(x => x.Capacity.HasValue).WithMessage("Sức chứa phải lớn hơn 0");
        }
    }
}
