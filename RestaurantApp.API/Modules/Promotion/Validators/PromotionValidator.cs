using FluentValidation;
using RestaurantApp.API.Modules.Promotion.Services;

namespace RestaurantApp.API.Modules.Promotion.Validators
{
    public class CreatePromotionValidator : AbstractValidator<CreatePromotionDto>
    {
        public CreatePromotionValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên khuyến mãi không được để trống");
            RuleFor(x => x.Type).NotEmpty().WithMessage("Loại khuyến mãi không hợp lệ");
            RuleFor(x => x.ApplyTo).NotEmpty().WithMessage("Đối tượng áp dụng không hợp lệ");
            RuleFor(x => x.DiscountValue).GreaterThan(0).WithMessage("Giá trị giảm giá phải lớn hơn 0");
            RuleFor(x => x.StartDate).NotEmpty().WithMessage("Ngày bắt đầu không được để trống");
            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("Ngày kết thúc không được để trống")
                .Must((dto, endDate) => endDate >= dto.StartDate)
                .WithMessage("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu");
        }
    }

    public class CreateVoucherValidator : AbstractValidator<CreateVoucherDto>
    {
        public CreateVoucherValidator()
        {
            RuleFor(x => x.Code).NotEmpty().WithMessage("Mã voucher không được để trống")
                .MinimumLength(3).WithMessage("Mã voucher ít nhất 3 ký tự");
            RuleFor(x => x.PromotionId).NotEmpty().WithMessage("Khuyến mãi không hợp lệ");
        }
    }
}
