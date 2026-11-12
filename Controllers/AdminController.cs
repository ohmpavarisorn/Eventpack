using System.Security.Cryptography;
using EventPack.Models.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventPack.Controllers;

public class AdminController(Csi402projectContext db, IConfiguration config) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? status)
    {
        if (!await CanManage()) return StatusCode(403);
        var bookings = db.Bookings.AsNoTracking().Include(b => b.UserIdNavigation).AsQueryable();
        if (status is "pending" or "confirmed" or "cancelled") bookings = bookings.Where(b => b.BookingStatus == status);
        ViewBag.Status = status;
        return View(await bookings.OrderByDescending(b => b.CreatedDate).Take(100).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBooking(string bookingId, string status)
    {
        if (!await CanManage()) return StatusCode(403);
        if (status is not ("confirmed" or "cancelled")) return BadRequest();
        var rows = await db.Bookings.Where(b => b.BookingId == bookingId && b.BookingStatus == "pending")
            .ExecuteUpdateAsync(update => update.SetProperty(b => b.BookingStatus, status));
        TempData["AdminMessage"] = rows == 1 ? "อัปเดตสถานะแล้ว" : "รายการนี้มีการเปลี่ยนสถานะแล้ว";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Menu()
    {
        if (!await CanManage()) return StatusCode(403);
        return View(await db.MenuItems.AsNoTracking().OrderBy(m => m.Category).ThenBy(m => m.ItemName).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMenu(string? id, string? itemName, string? category, decimal price)
    {
        if (!await CanManage()) return StatusCode(403);
        itemName = itemName?.Trim();
        category = category?.Trim();
        if (!ModelState.IsValid || string.IsNullOrEmpty(itemName) || itemName.Length > 100 ||
            string.IsNullOrEmpty(category) || category.Length > 50 || price is < 0 or > 100000)
        {
            TempData["AdminMessage"] = "กรอกชื่อ หมวดหมู่ และราคา 0–100,000 บาท";
            return RedirectToAction(nameof(Menu));
        }
        var item = string.IsNullOrEmpty(id) ? null : await db.MenuItems.FindAsync(id);
        if (item is null)
        {
            if (!string.IsNullOrEmpty(id)) return NotFound();
            item = new MenuItem { MenuItemId = Convert.ToHexString(RandomNumberGenerator.GetBytes(5)) };
            db.MenuItems.Add(item);
        }
        item.ItemName = itemName;
        item.Category = category;
        item.BasePrice = price;
        await db.SaveChangesAsync();
        TempData["AdminMessage"] = "บันทึกเมนูแล้ว";
        return RedirectToAction(nameof(Menu));
    }

    private async Task<bool> CanManage()
    {
        var id = HttpContext.Session.GetString("UserId");
        return id is not null && HttpContext.Session.GetString("Role") == "admin" &&
            string.Equals(id, config["Admin:UserId"], StringComparison.OrdinalIgnoreCase) &&
            await db.Userinformations.AnyAsync(user => user.UserId == id && user.Role == "admin");
    }
}
