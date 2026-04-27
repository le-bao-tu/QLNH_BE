using FluentValidation;
using RestaurantApp.API.Modules.Order.DTOs;

namespace RestaurantApp.API.Modules.Order.Validators
{
    public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderValidator()
        {
            RuleFor(x => x.TableId).NotEmpty().WithMessage("Vui lòng chọn bàn");
            RuleFor(x => x.RestaurantId).NotEmpty().WithMessage("ID nhà hàng không hợp lệ");
            RuleFor(x => x.GuestCount).GreaterThan(0).WithMessage("Số lượng khách phải lớn hơn 0");
            RuleFor(x => x.Items).NotEmpty().WithMessage("Đơn hàng phải có ít nhất một món ăn");
            RuleForEach(x => x.Items).SetValidator(new CreateOrderItemValidator());
        }
    }

    public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDto>
    {
        public CreateOrderItemValidator()
        {
            RuleFor(x => x.MenuItemId).NotEmpty().WithMessage("Sản phẩm không hợp lệ");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0");
        }
    }

    public class AddOrderItemValidator : AbstractValidator<AddOrderItemDto>
    {
        public AddOrderItemValidator()
        {
            RuleFor(x => x.MenuItemId).NotEmpty().WithMessage("Sản phẩm không hợp lệ");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0");
        }
    }

    public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        public UpdateOrderStatusValidator()
        {
            RuleFor(x => x.Status).NotEmpty().WithMessage("Trạng thái không được để trống");
        }
    }
}
