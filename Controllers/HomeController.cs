using EventManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EventManagement.Controllers;
public class HomeController : Controller {
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) { _db=db; }
    public async Task<IActionResult> Index() {
        var events = await _db.Events.Include(e=>e.Category).Include(e=>e.Registrations).Where(e=>e.EndTime>DateTime.Now).OrderBy(e=>e.StartTime).Take(3).ToListAsync();
        return View(events);
    }
    public IActionResult Error() { Response.StatusCode=500; return View(); }
}
