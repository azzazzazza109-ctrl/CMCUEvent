using System.ComponentModel.DataAnnotations;
namespace EventManagement.ViewModels;
public class EventForm {
    public int Id { get; set; }
    [Required(ErrorMessage="Nhập tên sự kiện."), StringLength(200)] public string Name { get; set; } = "";
    [Required(ErrorMessage="Nhập mô tả.")] public string Description { get; set; } = "";
    [Range(1,int.MaxValue,ErrorMessage="Chọn danh mục.")] public int CategoryId { get; set; }
    [Required(ErrorMessage="Chọn thời gian bắt đầu.")] public DateTime? StartTime { get; set; }
    [Required(ErrorMessage="Chọn thời gian kết thúc.")] public DateTime? EndTime { get; set; }
    [Required(ErrorMessage="Nhập địa điểm."),StringLength(250)] public string Location { get; set; } = "";
    [Range(1,int.MaxValue,ErrorMessage="Sức chứa phải lớn hơn 0.")] public int MaxParticipants { get; set; }
    public IFormFile? Image { get; set; }
    public string? CurrentImage { get; set; }
}
public class LoginForm {
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
public class RegisterForm {
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, StringLength(100,MinimumLength=6), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required, Compare(nameof(Password),ErrorMessage="Mật khẩu xác nhận không khớp."), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}
