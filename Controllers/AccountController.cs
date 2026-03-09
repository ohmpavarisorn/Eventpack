using Microsoft.AspNetCore.Mvc;
using EventPack.ViewModels;
using EventPack.Models.Db;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Security.Cryptography;
using System.Text;

namespace EventPack.Controllers;

public class AccountController : Controller
{
    private readonly Csi402projectContext Db;

    // 2. รับ Context ผ่าน Constructor (Dependency Injection)
    public AccountController(Csi402projectContext context)
    {
        Db = context;
    }
    // Hash passwordโดยใช้ SHA256
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    // ตรวจสอบ password
    private bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput.Equals(hash);
    }

    // แสดงหน้า Register
    
    public IActionResult Register()
    {
        return View();
    }

    // รับค่าจากฟอร์ม Register
    [HttpPost]
    public IActionResult Register(LabUserViewModels data)
    {
        if (data == null || string.IsNullOrEmpty(data.Name) || string.IsNullOrEmpty(data.Password))
        {
            return RedirectToAction("Register");
        }

        // บันทึกข้อมูลใน Userinformation
        var user = new Userinformation
        {
            UserId = data.Name,
            FullName = data.Name,
            UserPhone = data.Phone,
            UserAddress = data.Address,
            UserPassword = HashPassword(data.Password) // Hash password ก่อบันทึก
        };

        Db.Userinformations.Add(user);
        Db.SaveChanges();

        return RedirectToAction("userController", "Admin");
    }

    // หน้า Login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(LabUserViewModels data)
    {
        if (data == null || string.IsNullOrEmpty(data.Name) || string.IsNullOrEmpty(data.Password))
        {
            return RedirectToAction("Login");
        }

        // ตรวจสอบและเข้าสู่ระบบ
        var user = Db.Userinformations.FirstOrDefault(u => u.UserId == data.Name);
        if (user != null && VerifyPassword(data.Password, user.UserPassword))
        {
            return RedirectToAction("userController", "Admin");
        }

        return RedirectToAction("Login");
    }
    
    [HttpGet]
    public IActionResult Lab9()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Lab9(Lab9ViewModel data)
    {   
        var u = new labstudent();
        u.StdID = data.UserID;
        u.StdPASSWORD = data.Password;
        u.StdName = data.Name;
        u.STDLastname = data.Lastname;

        Db.Labstudents.Add(u);
        Db.SaveChanges();
        return RedirectToAction("Lab9List", "Account");
    }

    public IActionResult Lab9List()
    {
        var User = (from s in Db.Labstudents select new Lab9ViewModel
        {
            UserID = s.StdID,
            Password = s.StdPASSWORD,
            Name = s.StdName,
            Lastname = s.STDLastname
        }).ToList();
        return View(User);
    }
}
