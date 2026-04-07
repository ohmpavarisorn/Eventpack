using Microsoft.AspNetCore.Mvc;
using EventPack.Models.Db;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Security.Cryptography;
using System.Text;

namespace EventPack.Controllers;

public class AccountController : Controller
{
    private readonly Csi402projectContext Db;

    public AccountController(Csi402projectContext context)
    {
        Db = context;
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput.Equals(hash);
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(string name, string password, string phone = "", string address = "", string role = "customer")
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
        {
            ViewBag.ErrorMessage = "กรุณากรอกชื่อผู้ใช้และรหัสผ่าน";
            return View();
        }

        // Check if user already exists
        var existingUser = Db.Userinformations.FirstOrDefault(u => u.UserId == name);
        if (existingUser != null)
        {
            ViewBag.ErrorMessage = "ชื่อผู้ใช้นี้มีอยู่แล้ว กรุณาเลือกชื่ออื่น";
            return View();
        }

        var user = new Userinformation
        {
            UserId = name,
            FullName = name,
            UserPhone = phone,
            UserAddress = address,
            UserPassword = HashPassword(password),
            Role = role
        };

        try
        {
            Db.Userinformations.Add(user);
            Db.SaveChanges();

            TempData["SuccessMessage"] = "สร้างบัญชีสำเร็จ กรุณาเข้าสู่ระบบ";
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = $"เกิดข้อผิดพลาดในการสร้างบัญชี: {ex.Message}";
            return View();
        }
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string name, string password)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
        {
            ViewBag.ErrorMessage = "กรุณากรอกชื่อผู้ใช้และรหัสผ่าน";
            return View();
        }

        var user = Db.Userinformations.FirstOrDefault(u => u.UserId == name);
        
        if (user == null)
        {
            ViewBag.ErrorMessage = $"ไม่พบผู้ใช้ '{name}' ในระบบ โปรดสมัครสมาชิกก่อน";
            return View();
        }
        
        if (string.IsNullOrEmpty(user.UserPassword))
        {
            ViewBag.ErrorMessage = "เกิดข้อผิดพลาดในการตรวจสอบ โปรดลองใหม่";
            return View();
        }
        
        if (!VerifyPassword(password, user.UserPassword))
        {
            ViewBag.ErrorMessage = "รหัสผ่านไม่ถูกต้อง โปรดลองใหม่";
            return View();
        }

        HttpContext.Session.SetString("UserId", user.UserId);
        HttpContext.Session.SetString("FullName", user.FullName ?? "");
        HttpContext.Session.SetString("Role", user.Role ?? "customer");
        
        ViewBag.SuccessMessage = $"ยินดีต้อนรับ {user.FullName}! เข้าสู่ระบบสำเร็จ";
        
        return RedirectToAction("Index", "Home");
    }
    
    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("UserId");
        HttpContext.Session.Remove("FullName");
        HttpContext.Session.Remove("Role");
        return RedirectToAction("Index", "Home");
    }
}
