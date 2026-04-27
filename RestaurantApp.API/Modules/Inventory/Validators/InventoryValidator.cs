using FluentValidation;
using RestaurantApp.API.Modules.Inventory.Services;

namespace RestaurantApp.API.Modules.Inventory.Validators
{
    public class CreateSupplierValidator : AbstractValidator<CreateSupplierDto>
    {
        public CreateSupplierValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên nhà cung cấp không được để trống");
            RuleFor(x => x.RestaurantId).NotEmpty().WithMessage("ID nhà hàng không hợp lệ");
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Email không hợp lệ");
        }
    }

    public class CreateInventoryItemValidator : AbstractValidator<CreateInventoryItemDto>
    {
        public CreateInventoryItemValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên nguyên liệu không được để trống");
            RuleFor(x => x.Unit).NotEmpty().WithMessage("Đơn vị tính không được để trống");
            RuleFor(x => x.BranchId).NotEmpty().WithMessage("ID chi nhánh không hợp lệ");
            RuleFor(x => x.MinQuantity).GreaterThanOrEqualTo(0).WithMessage("Số lượng tối thiểu không được âm");
            RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0).WithMessage("Giá nhập không được âm");
        }
    }

    public class InventoryTransactionValidator : AbstractValidator<InventoryTransactionDto>
    {
        public InventoryTransactionValidator()
        {
            RuleFor(x => x.InventoryItemId).NotEmpty().WithMessage("Nguyên liệu không hợp lệ");
            RuleFor(x => x.Type).NotEmpty().WithMessage("Loại giao dịch không hợp lệ");
            RuleFor(x => x.QuantityChange).NotEqual(0).WithMessage("Số lượng thay đổi phải khác 0");
        }
    }
}
