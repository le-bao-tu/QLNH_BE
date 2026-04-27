using FluentValidation;
using RestaurantApp.API.Modules.Reservation.DTOs;

namespace RestaurantApp.API.Modules.Reservation.Validators
{
    public class CreateReservationValidator : AbstractValidator<CreateReservationDto>
    {
        public CreateReservationValidator()
        {
            RuleFor(x => x.BranchId).NotEmpty().WithMessage("Vui lòng chọn chi nhánh");
            RuleFor(x => x.GuestName).NotEmpty().WithMessage("Họ tên khách hàng không được để trống");
            RuleFor(x => x.GuestPhone).NotEmpty().WithMessage("Số điện thoại khách hàng không được để trống")
                .Matches(@"^\+?[0-9]{10,12}$").WithMessage("Số điện thoại không hợp lệ");
            RuleFor(x => x.PartySize).GreaterThan(0).WithMessage("Số lượng khách phải lớn hơn 0");
            RuleFor(x => x.ReservedAt).GreaterThan(DateTime.UtcNow).WithMessage("Thời gian đặt bàn phải trong tương lai");
        }
    }

    public class UpdateReservationStatusValidator : AbstractValidator<UpdateReservationStatusDto>
    {
        public UpdateReservationStatusValidator()
        {
            RuleFor(x => x.Status).NotEmpty().WithMessage("Trạng thái không được để trống");
        }
    }
}
