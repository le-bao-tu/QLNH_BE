using FluentValidation;
using RestaurantApp.API.Modules.Menu.DTOs;

namespace RestaurantApp.API.Modules.Menu.Validators
{
    public class CreateMenuCategoryValidator : AbstractValidator<CreateMenuCategoryDto>
    {
        public CreateMenuCategoryValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên danh mục không được để trống");
            RuleFor(x => x.RestaurantId).NotEmpty().WithMessage("Nhà hàng không hợp lệ");
        }
    }

    public class CreateMenuItemValidator : AbstractValidator<CreateMenuItemDto>
    {
        public CreateMenuItemValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên món không được để trống");
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Vui lòng chọn danh mục");
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Giá món không được âm");
            
            RuleFor(x => x.ComboItemIds)
                .NotEmpty().WithMessage("Vui lòng chọn ít nhất một món cho Combo")
                .When(x => x.ItemType == "combo");
        }
    }
}
