using EventManagement.Data;
using EventManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EventManagement.Controllers;
[Authorize(Roles="Admin")]
public class CategoriesController : Controller {
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) { _db=db; }
    public async Task<IActionResult> Index() => View(await _db.Categories.OrderBy(c=>c.Name).ToListAsync());
    public async Task<IActionResult> Details(int id) { var c=await _db.Categories.FindAsync(id); return c==null?NotFound():View(c); }
    [HttpGet] public IActionResult Create() => View(new Category());
    [HttpPost] public async Task<IActionResult> Create([Bind("Name,Description")] Category c) {
        if (ModelState.IsValid && await _db.Categories.AnyAsync(x=>x.Name==c.Name.Trim())) ModelState.AddModelError("Name","Tên danh mục đã tồn tại.");
        if (!ModelState.IsValid) return View(c);
        c.Name=c.Name.Trim(); _db.Categories.Add(c);
        try { await _db.SaveChangesAsync(); } catch(DbUpdateException) { ModelState.AddModelError("","Không lưu được danh mục. Kiểm tra tên trùng."); return View(c); }
        TempData["Success"]="Đã thêm danh mục."; return RedirectToAction(nameof(Index));
    }
    [HttpGet] public async Task<IActionResult> Edit(int id) { var c=await _db.Categories.FindAsync(id); return c==null?NotFound():View(c); }
    [HttpPost] public async Task<IActionResult> Edit(int id,[Bind("Name,Description")] Category form) {
        var c=await _db.Categories.FindAsync(id); if(c==null) return NotFound();
        form.Id=id;
        if (ModelState.IsValid && await _db.Categories.AnyAsync(x=>x.Id!=id && x.Name==form.Name.Trim())) ModelState.AddModelError("Name","Tên danh mục đã tồn tại.");
        if (!ModelState.IsValid) return View(form);
        c.Name=form.Name.Trim(); c.Description=form.Description;
        try { await _db.SaveChangesAsync(); } catch(DbUpdateException) { ModelState.AddModelError("","Không cập nhật được danh mục."); return View(form); }
        TempData["Success"]="Đã cập nhật danh mục."; return RedirectToAction(nameof(Index));
    }
    [HttpGet] public async Task<IActionResult> Delete(int id) { var c=await _db.Categories.FindAsync(id); return c==null?NotFound():View(c); }
    [HttpPost, ActionName("Delete")] public async Task<IActionResult> DeleteConfirmed(int id) {
        var c=await _db.Categories.FindAsync(id); if(c==null) return NotFound();
        // Không xóa danh mục đang được dùng.
        if (await _db.Events.AnyAsync(e=>e.CategoryId==id)) { TempData["Error"]="Danh mục đang chứa sự kiện."; return RedirectToAction(nameof(Index)); }
        _db.Categories.Remove(c);
        try { await _db.SaveChangesAsync(); TempData["Success"]="Đã xóa danh mục."; } catch(DbUpdateException) { TempData["Error"]="Không thể xóa danh mục đang được sử dụng."; }
        return RedirectToAction(nameof(Index));
    }
}
