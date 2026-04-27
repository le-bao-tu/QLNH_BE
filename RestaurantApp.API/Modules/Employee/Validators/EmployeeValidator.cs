using FluentValidation;
using RestaurantApp.API.Modules.Employee.Services;

namespace RestaurantApp.API.Modules.Employee.Validators
{
    public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeDto>
    {
        public CreateEmployeeValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ tên không được để trống")
                .MaximumLength(100).WithMessage("Họ tên không được vượt quá 100 ký tự");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Email không hợp lệ");

            RuleFor(x => x.Phone)
                .Matches(@"^\+?[0-9]{10,12}$").When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage("Số điện thoại không hợp lệ (10-12 số)");

            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Vui lòng chọn vai trò");

            RuleFor(x => x.HourlyRate)
                .GreaterThanOrEqualTo(0).WithMessage("Lương theo giờ không được âm");

            RuleFor(x => x.BranchId)
                .NotEmpty().WithMessage("Chi nhánh không được để trống");
        }
    }

    public class CreateShiftValidator : AbstractValidator<CreateShiftDto>
    {
        public CreateShiftValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Vui lòng chọn nhân viên");
            RuleFor(x => x.ShiftDate).NotEmpty().WithMessage("Ngày làm việc không được để trống");
            RuleFor(x => x.StartTime).NotEmpty().WithMessage("Giờ bắt đầu không được để trống");
            RuleFor(x => x.EndTime).NotEmpty().WithMessage("Giờ kết thúc không được để trống");
            RuleFor(x => x.BranchId).NotEmpty().WithMessage("Chi nhánh không được để trống");
            
            RuleFor(x => x)
                .Must(x => x.EndTime > x.StartTime)
                .WithMessage("Giờ kết thúc phải sau giờ bắt đầu")
                .When(x => x.StartTime != default && x.EndTime != default);
        }
    }
}
