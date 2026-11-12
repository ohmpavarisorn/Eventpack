using System.Security.Cryptography;
using System.Text;
using EventPack.Models.Db;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventPack.Controllers;

public class AccountController(Csi402projectContext db, IPasswordHasher<Userinformation> passwords, IConfiguration config) : Controller
{
    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string? name, string? password, string? confirmPassword, string? phone, string? address)
    {
        name = name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 10 || password is null ||
            password.Length is < 8 or > 128 || password != confirmPassword || phone?.Length > 15 || address?.Length > 255)
        {
            ViewBag.ErrorMessage = "ใช้ชื่อผู้ใช้ไม่เกิน 10 ตัวอักษร รหัสผ่าน 8–128 ตัวอักษร และกรอกข้อมูลให้ครบ";
            return View();
        }
        if (string.Equals(name, config["Admin:UserId"], StringComparison.OrdinalIgnoreCase) ||
            await db.Userinformations.AnyAsync(u => u.UserId == name))
        {
            ViewBag.ErrorMessage = "ชื่อผู้ใช้นี้มีอยู่แล้ว กรุณาเลือกชื่ออื่น";
            return View();
        }
        var user = new Userinformation { UserId = name, FullName = name, UserPhone = phone, UserAddress = address, Role = "customer" };
        user.UserPassword = passwords.HashPassword(user, password);
        db.Userinformations.Add(user);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            ViewBag.ErrorMessage = "สร้างบัญชีไม่สำเร็จ กรุณาใช้ชื่ออื่นหรือลองอีกครั้ง";
            return View();
        }
        TempData["SuccessMessage"] = "สร้างบัญชีสำเร็จ กรุณาเข้าสู่ระบบ";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string? name, string? password)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(password) || password.Length > 128)
            return InvalidLogin();

        var user = await db.Userinformations.FindAsync(name.Trim());
        if (user is null || string.IsNullOrEmpty(user.UserPassword)) return InvalidLogin();
        var result = passwords.VerifyHashedPassword(user, user.UserPassword, password);
        // Existing accounts used a base64-encoded SHA-256 digest. Upgrade a verified legacy password on login.
        if (result == PasswordVerificationResult.Failed && user.UserPassword.Length == 44 &&
            CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(user.UserPassword),
                Encoding.ASCII.GetBytes(Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password))))))
            result = PasswordVerificationResult.SuccessRehashNeeded;
        if (result == PasswordVerificationResult.Failed) return InvalidLogin();
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.UserPassword = passwords.HashPassword(user, password);
            await db.SaveChangesAsync();
        }

        var isAdmin = user.Role == "admin" && string.Equals(user.UserId, config["Admin:UserId"], StringComparison.OrdinalIgnoreCase);
        HttpContext.Session.Clear();
        HttpContext.Session.SetString("UserId", user.UserId);
        HttpContext.Session.SetString("FullName", user.FullName ?? user.UserId);
        HttpContext.Session.SetString("Role", isAdmin ? "admin" : "customer");
        return isAdmin ? RedirectToAction("Index", "Admin") : RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    private IActionResult InvalidLogin()
    {
        ViewBag.ErrorMessage = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง";
        return View("Login");
    }
}
