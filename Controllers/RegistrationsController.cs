using System.Data;
using System.Data.Common;
using EventManagement.Data;
using EventManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EventManagement.Controllers;
[Authorize(Roles="User")]
public class RegistrationsController : Controller {
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    public RegistrationsController(AppDbContext db,UserManager<ApplicationUser> users) { _db=db; _users=users; }
    public async Task<IActionResult> Index() {
        var userId=_users.GetUserId(User);
        return View(await _db.Registrations.Include(r=>r.Event).ThenInclude(e=>e.Category).Where(r=>r.UserId==userId).OrderByDescending(r=>r.RegisteredAt).ToListAsync());
    }
    [HttpPost] public async Task<IActionResult> Create(int eventId) {
        // Serializable giữ các điều kiện đọc ổn định trong lúc đăng ký.
        try {
            await using var tx=await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var e=await _db.Events.FindAsync(eventId);
            if(e==null) return NotFound();
            var userId=_users.GetUserId(User)!;
            var existing=await _db.Registrations.SingleOrDefaultAsync(r=>r.EventId==eventId && r.UserId==userId);
            int count=await _db.Registrations.CountAsync(r=>r.EventId==eventId && r.Status=="Registered");
            if(existing?.Status=="Registered") TempData["Error"]="Bạn đã đăng ký sự kiện này.";
            else if(e.Status!="Open" || e.StartTime<=DateTime.Now) TempData["Error"]="Sự kiện đã đóng hoặc hết hạn đăng ký.";
            else if(count>=e.MaxParticipants) TempData["Error"]="Sự kiện đã hết chỗ.";
            else {
                if(existing==null) _db.Registrations.Add(new Registration { EventId=eventId,UserId=userId,RegisteredAt=DateTime.Now });
                else { existing.Status="Registered"; existing.RegisteredAt=DateTime.Now; existing.CancelledAt=null; }
                await _db.SaveChangesAsync(); await tx.CommitAsync();
                TempData["Success"]="Đăng ký thành công."; return RedirectToAction(nameof(Index));
            }
        } catch (Exception ex) when (ex is DbUpdateException || ex is DbException) {
            TempData["Error"]="Không thể lưu đăng ký. Có thể chỗ vừa được người khác đăng ký; hãy kiểm tra lại.";
        }
        return RedirectToAction("Details","Events",new { id=eventId });
    }
    [HttpPost] public async Task<IActionResult> Cancel(int id) {
        try {
            await using var tx=await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var userId=_users.GetUserId(User);
            var r=await _db.Registrations.Include(r=>r.Event).SingleOrDefaultAsync(r=>r.Id==id && r.UserId==userId);
            if(r==null) return NotFound();
            if(r.Status!="Registered" || r.Event.StartTime<=DateTime.Now) { TempData["Error"]="Không thể hủy đăng ký này."; return RedirectToAction(nameof(Index)); }
            r.Status="Cancelled"; r.CancelledAt=DateTime.Now;
            await _db.SaveChangesAsync(); await tx.CommitAsync(); TempData["Success"]="Đã hủy đăng ký.";
        } catch (Exception ex) when (ex is DbUpdateException || ex is DbException) { TempData["Error"]="Không thể hủy lúc này. Hãy thử lại."; }
        return RedirectToAction(nameof(Index));
    }
}
