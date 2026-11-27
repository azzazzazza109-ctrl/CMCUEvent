using System.Data;
using System.Data.Common;
using EventManagement.Data;
using EventManagement.Models;
using EventManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace EventManagement.Controllers;
[Authorize(Roles="Admin")]
public class AdminEventsController : Controller {
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    public AdminEventsController(AppDbContext db,IWebHostEnvironment env) { _db=db; _env=env; }
    public async Task<IActionResult> Index(string? keyword,int? categoryId,int page=1) {
        var query=_db.Events.Include(e=>e.Category).Include(e=>e.Registrations).AsQueryable();
        if(!string.IsNullOrWhiteSpace(keyword)) query=query.Where(e=>e.Name.Contains(keyword.Trim()));
        if(categoryId.HasValue) query=query.Where(e=>e.CategoryId==categoryId);
        int count=await query.CountAsync(),pages=Math.Max(1,(int)Math.Ceiling(count/6.0)); page=Math.Clamp(page,1,pages);
        return View(new EventListViewModel { Items=await query.OrderByDescending(e=>e.CreatedAt).ThenBy(e=>e.Id).Skip((page-1)*6).Take(6).ToListAsync(),Categories=await _db.Categories.OrderBy(c=>c.Name).ToListAsync(),Keyword=keyword,CategoryId=categoryId,CurrentPage=page,TotalPages=pages,TotalItems=count });
    }
    public async Task<IActionResult> Details(int id) { var e=await _db.Events.Include(e=>e.Category).Include(e=>e.Registrations).FirstOrDefaultAsync(e=>e.Id==id); return e==null?NotFound():View(e); }
    public async Task<IActionResult> Participants(int id) { var e=await _db.Events.Include(e=>e.Registrations).ThenInclude(r=>r.User).FirstOrDefaultAsync(e=>e.Id==id); return e==null?NotFound():View(e); }
    private async Task LoadCategories(int selected=0) { ViewBag.Categories=new SelectList(await _db.Categories.OrderBy(c=>c.Name).ToListAsync(),"Id","Name",selected); }
    [HttpGet] public async Task<IActionResult> Create() { await LoadCategories(); return View(new EventForm { StartTime=DateTime.Today.AddDays(1).AddHours(9),EndTime=DateTime.Today.AddDays(1).AddHours(11),MaxParticipants=30 }); }
    [HttpPost] public async Task<IActionResult> Create(EventForm form) {
        form.CurrentImage=null;
        await ValidateForm(form);
        if(!ModelState.IsValid) { await LoadCategories(form.CategoryId); return View(form); }
        var e=new Event(); CopyFields(e,form);
        string? newImage=null;
        try { if(form.Image!=null) { newImage=await SaveImage(form.Image); e.ImagePath=newImage; } _db.Events.Add(e); await _db.SaveChangesAsync(); }
        catch(Exception ex) when(ex is DbUpdateException || ex is IOException || ex is UnauthorizedAccessException) { RemoveImage(newImage); ModelState.AddModelError("","Không lưu được sự kiện."); await LoadCategories(form.CategoryId); return View(form); }
        TempData["Success"]="Đã thêm sự kiện."; return RedirectToAction(nameof(Index));
    }
    [HttpGet] public async Task<IActionResult> Edit(int id) {
        var e=await _db.Events.FindAsync(id); if(e==null) return NotFound(); await LoadCategories(e.CategoryId);
        return View(new EventForm { Id=e.Id,Name=e.Name,Description=e.Description,CategoryId=e.CategoryId,StartTime=e.StartTime,EndTime=e.EndTime,Location=e.Location,MaxParticipants=e.MaxParticipants,CurrentImage=e.ImagePath });
    }
    [HttpPost] public async Task<IActionResult> Edit(int id,EventForm form) {
        form.Id=id; string? newImage=null; string? oldImage=null;
        try {
            await using var tx=await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var e=await _db.Events.FindAsync(id); if(e==null) return NotFound(); form.CurrentImage=e.ImagePath;
            await ValidateForm(form);
            int count=await _db.Registrations.CountAsync(r=>r.EventId==id && r.Status=="Registered");
            if(form.MaxParticipants<count) ModelState.AddModelError("MaxParticipants","Sức chứa không được thấp hơn số đã đăng ký.");
            if(!ModelState.IsValid) { await LoadCategories(form.CategoryId); return View(form); }
            oldImage=e.ImagePath; CopyFields(e,form);
            if(form.Image!=null) { newImage=await SaveImage(form.Image); e.ImagePath=newImage; }
            await _db.SaveChangesAsync(); await tx.CommitAsync();
        } catch(Exception ex) when(ex is DbUpdateException || ex is DbException || ex is IOException || ex is UnauthorizedAccessException) {
            RemoveImage(newImage); ModelState.AddModelError("","Không thể cập nhật. Hãy kiểm tra và thử lại."); await LoadCategories(form.CategoryId); return View(form);
        }
        if(newImage!=null) RemoveImage(oldImage); TempData["Success"]="Đã cập nhật sự kiện."; return RedirectToAction(nameof(Index));
    }
    [HttpGet] public async Task<IActionResult> Delete(int id) { var e=await _db.Events.FindAsync(id); return e==null?NotFound():View(e); }
    [HttpPost,ActionName("Delete")] public async Task<IActionResult> DeleteConfirmed(int id) {
        var e=await _db.Events.FindAsync(id); if(e==null) return NotFound();
        if(await _db.Registrations.AnyAsync(r=>r.EventId==id)) { TempData["Error"]="Sự kiện đã có lịch sử đăng ký. Hãy đóng thay vì xóa."; return RedirectToAction(nameof(Index)); }
        _db.Events.Remove(e);
        try { await _db.SaveChangesAsync(); RemoveImage(e.ImagePath); TempData["Success"]="Đã xóa sự kiện."; } catch(DbUpdateException) { TempData["Error"]="Không thể xóa sự kiện đã có đăng ký."; }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost] public Task<IActionResult> Close(int id) => ChangeStatus(id,false);
    [HttpPost] public Task<IActionResult> Open(int id) => ChangeStatus(id,true);
    private async Task<IActionResult> ChangeStatus(int id,bool open) {
        var e=await _db.Events.FindAsync(id); if(e==null) return NotFound();
        if(open && e.StartTime<=DateTime.Now) TempData["Error"]="Sự kiện đã bắt đầu, không thể mở đăng ký.";
        else { e.Status=open?"Open":"Closed"; await _db.SaveChangesAsync(); TempData["Success"]="Đã cập nhật trạng thái."; }
        return RedirectToAction(nameof(Index));
    }
    // Kiểm tra ngày, danh mục và ảnh ở phía máy chủ.
    private async Task ValidateForm(EventForm f) {
        if(f.StartTime.HasValue && f.EndTime.HasValue && f.EndTime<=f.StartTime) ModelState.AddModelError("EndTime","Ngày kết thúc phải sau ngày bắt đầu.");
        if(!await _db.Categories.AnyAsync(c=>c.Id==f.CategoryId)) ModelState.AddModelError("CategoryId","Danh mục không tồn tại.");
        if(f.Image!=null) {
            var ext=Path.GetExtension(f.Image.FileName).ToLowerInvariant();
            if(f.Image.Length<=0 || f.Image.Length>2*1024*1024 || (ext!=".jpg" && ext!=".jpeg" && ext!=".png")) ModelState.AddModelError("Image","Chỉ nhận ảnh JPG hoặc PNG tối đa 2 MB.");
            else {
                using var stream=f.Image.OpenReadStream(); byte[] b=new byte[8]; int n=await stream.ReadAsync(b.AsMemory());
                bool png=n==8 && b.SequenceEqual(new byte[]{137,80,78,71,13,10,26,10});
                bool jpg=n>=3 && b[0]==255 && b[1]==216 && b[2]==255;
                if(ext==".png"?!png:!jpg) ModelState.AddModelError("Image","Nội dung ảnh không đúng định dạng.");
            }
        }
    }
    private static void CopyFields(Event e,EventForm f) { e.Name=f.Name.Trim(); e.Description=f.Description.Trim(); e.CategoryId=f.CategoryId; e.StartTime=f.StartTime!.Value; e.EndTime=f.EndTime!.Value; e.Location=f.Location.Trim(); e.MaxParticipants=f.MaxParticipants; }
    private async Task<string> SaveImage(IFormFile image) {
        string name=Guid.NewGuid().ToString("N")+Path.GetExtension(image.FileName).ToLowerInvariant(); string dir=Path.Combine(_env.WebRootPath,"uploads"); Directory.CreateDirectory(dir);
        string path=Path.Combine(dir,name);
        try { using var stream=System.IO.File.Create(path); await image.CopyToAsync(stream); }
        catch { if(System.IO.File.Exists(path)) System.IO.File.Delete(path); throw; }
        return "/uploads/"+name;
    }
    private void RemoveImage(string? url) {
        if(url==null || !url.StartsWith("/uploads/",StringComparison.Ordinal)) return;
        var name=Path.GetFileName(url); var path=Path.Combine(_env.WebRootPath,"uploads",name);
        try { if(System.IO.File.Exists(path)) System.IO.File.Delete(path); } catch(IOException) { } catch(UnauthorizedAccessException) { }
    }
}
