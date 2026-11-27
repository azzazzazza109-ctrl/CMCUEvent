using EventManagement.Data;
using EventManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EventManagement.Controllers;
public class EventsController : Controller {
    private readonly AppDbContext _db;
    public EventsController(AppDbContext db) { _db=db; }
    public async Task<IActionResult> Index(string? keyword, int? categoryId, int page=1) {
        var query = _db.Events.Include(e=>e.Category).Include(e=>e.Registrations).AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword)) query=query.Where(e=>e.Name.Contains(keyword.Trim()));
        if (categoryId.HasValue) query=query.Where(e=>e.CategoryId==categoryId);
        int count=await query.CountAsync(), pages=Math.Max(1,(int)Math.Ceiling(count/6.0));
        page=Math.Clamp(page,1,pages);
        return View(new EventListViewModel { Items=await query.OrderBy(e=>e.StartTime).ThenBy(e=>e.Id).Skip((page-1)*6).Take(6).ToListAsync(), Categories=await _db.Categories.OrderBy(c=>c.Name).ToListAsync(), Keyword=keyword, CategoryId=categoryId, CurrentPage=page, TotalPages=pages, TotalItems=count });
    }
    public async Task<IActionResult> Details(int id) {
        var e=await _db.Events.Include(e=>e.Category).Include(e=>e.Registrations).FirstOrDefaultAsync(e=>e.Id==id);
        return e==null ? NotFound() : View(e);
    }
    [HttpGet("/api/events/availability/{id:int}")]
    public async Task<IActionResult> Availability(int id) {
        var e=await _db.Events.Include(e=>e.Registrations).FirstOrDefaultAsync(e=>e.Id==id);
        return e==null ? NotFound() : Json(new { remaining=e.Remaining, status=e.DisplayStatus });
    }
}
