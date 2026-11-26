using EventManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace EventManagement.Data;
public static class SeedData {
    public static async Task InitializeAsync(IServiceProvider services, bool demo) {
        var db = services.GetRequiredService<AppDbContext>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var config = services.GetRequiredService<IConfiguration>();
        foreach (var role in new[] { "Admin", "User" }) {
            if (!await roles.RoleExistsAsync(role)) Check(await roles.CreateAsync(new IdentityRole(role)));
        }
        async Task<ApplicationUser> CreateUser(string email, string name, string role, string password) {
            var user = await users.FindByEmailAsync(email);
            if (user == null) {
                user = new ApplicationUser { UserName=email, Email=email, FullName=name };
                Check(await users.CreateAsync(user, password));
                Check(await users.AddToRoleAsync(user, role));
            } else if (!await users.IsInRoleAsync(user, role)) {
                throw new InvalidOperationException("Email đã tồn tại với vai trò khác; không tự nâng quyền.");
            }
            return user;
        }
        var adminEmail = config["Seed:AdminEmail"] ?? (demo ? "admin@event.test" : throw new InvalidOperationException("Thiếu Seed:AdminEmail"));
        var adminPassword = config["Seed:AdminPassword"] ?? throw new InvalidOperationException("Thiếu Seed:AdminPassword");
        await CreateUser(adminEmail, "Ban tổ chức", "Admin", adminPassword);
        if (!demo) return;
        var studentPassword = config["Seed:UserPassword"] ?? throw new InvalidOperationException("Thiếu Seed:UserPassword");
        var student = await CreateUser("student@event.test", "Sinh viên mẫu", "User", studentPassword);
        if (await db.Events.AnyAsync()) return;
        var categories = new[] { "Hội thảo", "Văn nghệ", "Thể thao", "Cuộc thi" };
        foreach (var name in categories) if (!await db.Categories.AnyAsync(c => c.Name == name)) db.Categories.Add(new Category { Name=name });
        await db.SaveChangesAsync();
        var names = new[] { "Hội thảo công nghệ thông tin", "Giải bóng đá sinh viên", "Đêm nhạc sinh viên", "Workshop kỹ năng viết CV", "Cuộc thi lập trình", "Giải cầu lông", "Hội thảo trí tuệ nhân tạo", "Cuộc thi tài năng sinh viên" };
        var categoryIndexes = new[] { 0,2,1,0,3,2,0,3 };
        var capacity = new[] { 100,1,200,50,80,40,150,120 };
        var images = new[] { "talk", "music", "sport", "contest" };
        for (int i=0; i<names.Length; i++) {
            var category = await db.Categories.SingleAsync(c => c.Name == categories[categoryIndexes[i]]);
            var start = DateTime.Today.AddDays(i==7 ? -2 : i+2).AddHours(9);
            var e = new Event { CategoryId=category.Id, Name=names[i], Description="Sự kiện dành cho sinh viên. Hãy đến đúng giờ và tuân thủ hướng dẫn của ban tổ chức.", Location="Cơ sở Đại học CMC – phòng sinh hoạt chung (dữ liệu mẫu)", StartTime=start, EndTime=start.AddHours(2), MaxParticipants=capacity[i], Status=i==6 ? "Closed" : "Open", ImagePath="/images/"+images[categoryIndexes[i]]+".svg" };
            if (i==1) e.Registrations.Add(new Registration { UserId=student.Id, RegisteredAt=DateTime.Now, Status="Registered" });
            db.Events.Add(e);
        }
        await db.SaveChangesAsync();
    }
    private static void Check(IdentityResult result) {
        if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
