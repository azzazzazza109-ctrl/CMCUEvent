using EventManagement.Models;
using EventManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace EventManagement.Controllers;
public class AccountController : Controller {
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    public AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) { _users=users; _signIn=signIn; }
    [HttpGet, AllowAnonymous] public IActionResult Login(string? returnUrl) => View(new LoginForm { ReturnUrl=returnUrl });
    [HttpPost, AllowAnonymous] public async Task<IActionResult> Login(LoginForm form) {
        if (!ModelState.IsValid) return View(form);
        var result = await _signIn.PasswordSignInAsync(form.Email.Trim(), form.Password, false, true);
        if (!result.Succeeded) { ModelState.AddModelError("", "Đăng nhập không thành công. Kiểm tra thông tin hoặc thử lại sau."); return View(form); }
        if (Url.IsLocalUrl(form.ReturnUrl)) return LocalRedirect(form.ReturnUrl!);
        var user = await _users.FindByEmailAsync(form.Email.Trim());
        if (user != null && await _users.IsInRoleAsync(user,"Admin")) return RedirectToAction("Index","AdminEvents");
        return RedirectToAction("Index","Home");
    }
    [HttpGet, AllowAnonymous] public IActionResult Register() => View(new RegisterForm());
    [HttpPost, AllowAnonymous] public async Task<IActionResult> Register(RegisterForm form) {
        if (!ModelState.IsValid) return View(form);
        // Tài khoản tự đăng ký luôn là User, không nhận Role từ form.
        var user = new ApplicationUser { FullName=form.FullName.Trim(), Email=form.Email.Trim(), UserName=form.Email.Trim() };
        var result = await _users.CreateAsync(user,form.Password);
        if (result.Succeeded) {
            var role = await _users.AddToRoleAsync(user,"User");
            if (role.Succeeded) { TempData["Success"]="Tạo tài khoản thành công. Hãy đăng nhập."; return RedirectToAction(nameof(Login)); }
            await _users.DeleteAsync(user);
            result=role;
        }
        // Chuyển thông báo lỗi của Identity sang tiếng Việt.
        var messages = new HashSet<string>();
        foreach (var error in result.Errors)
        {
            string message = error.Code switch
            {
                "PasswordTooShort" => "Mật khẩu phải có ít nhất 6 ký tự.",
                "PasswordRequiresNonAlphanumeric" => "Mật khẩu phải chứa ít nhất một ký tự đặc biệt (ví dụ: @, #, !).",
                "PasswordRequiresLower" => "Mật khẩu phải chứa ít nhất một chữ cái thường (a-z).",
                "PasswordRequiresUpper" => "Mật khẩu phải chứa ít nhất một chữ cái hoa (A-Z).",
                "PasswordRequiresDigit" => "Mật khẩu phải chứa ít nhất một chữ số (0-9).",
                "DuplicateEmail" or "DuplicateUserName" => "Email này đã được đăng ký.",
                "InvalidEmail" or "InvalidUserName" => "Email không hợp lệ.",
                _ => "Không thể tạo tài khoản. Vui lòng kiểm tra thông tin và thử lại."
            };
            if (messages.Add(message)) ModelState.AddModelError("", message);
        }
        return View(form);
    }
    [HttpPost, Authorize] public async Task<IActionResult> Logout() { await _signIn.SignOutAsync(); return RedirectToAction("Index","Home"); }
    [HttpGet, AllowAnonymous] public IActionResult AccessDenied() { Response.StatusCode=403; return View(); }
}
