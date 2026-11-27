using System.ComponentModel.DataAnnotations;
namespace EventManagement.Models;
public class Category {
    public int Id { get; set; }
    [Required(ErrorMessage="Nhập tên danh mục."), StringLength(100)] public string Name { get; set; } = "";
    [StringLength(500)] public string? Description { get; set; }
}
