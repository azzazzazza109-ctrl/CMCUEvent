using System.ComponentModel.DataAnnotations;
namespace EventManagement.Models;
public class Registration {
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    [Required] public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    public DateTime RegisteredAt { get; set; }
    [StringLength(30)] public string Status { get; set; } = "Registered";
    public DateTime? CancelledAt { get; set; }
}
