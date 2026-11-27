using System.ComponentModel.DataAnnotations;
namespace EventManagement.Models;
public class Event {
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    [Required, StringLength(200)] public string Name { get; set; } = "";
    [Required] public string Description { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    [Required, StringLength(250)] public string Location { get; set; } = "";
    [Range(1, int.MaxValue)] public int MaxParticipants { get; set; }
    [StringLength(500)] public string ImagePath { get; set; } = "/images/default.svg";
    [StringLength(30)] public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<Registration> Registrations { get; set; } = new();
    // Chỉ đếm những đăng ký chưa bị hủy.
    public int RegisteredCount => Registrations.Count(r => r.Status == "Registered");
    public int Remaining => Math.Max(0, MaxParticipants - RegisteredCount);
    public string DisplayStatus => DateTime.Now >= EndTime ? "Đã kết thúc" : DateTime.Now >= StartTime ? "Đang diễn ra" : Status == "Closed" ? "Đã đóng" : Remaining == 0 ? "Hết chỗ" : "Đang mở";
}
