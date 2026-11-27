using EventManagement.Models;
namespace EventManagement.ViewModels;
public class EventListViewModel {
    public List<Event> Items { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
}
