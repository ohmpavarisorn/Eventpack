using System.Security.Cryptography;
using System.Text.Json;
using EventPack.Models.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventPack.Controllers;

public class BookingController(Csi402projectContext db) : Controller
{
    private static readonly int[][] FeeTiers = [[2,10,5000],[11,20,9000],[21,35,12000],[36,50,18000],[60,80,35000],[81,200,50000]];

    [HttpGet]
    public async Task<IActionResult> Create(int guests = 6, bool resume = false)
    {
        var customer = await CurrentCustomer();
        ViewBag.GuestPreview = customer is null;
        ViewBag.Resume = resume && customer is not null;
        if (!FeeTiers.Any(tier => guests >= tier[0] && guests <= tier[1])) guests = 6;
        return await ShowForm(new BookingViewModel { TotalGuests = guests, EventLocation = customer?.UserAddress, UserPhone = customer?.UserPhone });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingViewModel model)
    {
        if (await CurrentCustomer() is not { } customer) return RedirectToAction("Login", "Account");
        var menuIds = Request.Form.Keys.Where(key => key.StartsWith("menu_") && !key.StartsWith("menu_qty_"))
            .Select(key => key[5..]).ToList();
        var menu = await db.MenuItems.Where(item => menuIds.Contains(item.MenuItemId)).ToListAsync();
        if (menu.Count == 0 || menu.Count != menuIds.Count)
            ModelState.AddModelError("", "กรุณาเลือกอาหารจากเมนูอย่างน้อยหนึ่งรายการ");
        var tier = FeeTiers.FirstOrDefault(value => model.TotalGuests >= value[0] && model.TotalGuests <= value[1]);
        if (tier is null) ModelState.AddModelError("", "ระบบรองรับจำนวนแขก 2–50 หรือ 60–200 คน");
        if (model.EventDate is null || model.EventDate.Value.Date < DateTime.Today)
            ModelState.AddModelError("", "กรุณาเลือกวันจัดงานตั้งแต่วันนี้เป็นต้นไป");
        if (string.IsNullOrWhiteSpace(model.EventLocation) || model.EventLocation.Length > 255 ||
            string.IsNullOrWhiteSpace(model.UserPhone) || model.UserPhone.Length > 15)
            ModelState.AddModelError("", "กรุณากรอกสถานที่จัดงานและเบอร์ติดต่อ");
        if (!ModelState.IsValid) return await ShowForm(model);

        var pricePerPerson = menu.Sum(item => item.BasePrice);
        var operationFee = tier![2];
        var totalMenuPrice = pricePerPerson * model.TotalGuests;
        var booking = new Booking
        {
            BookingId = Convert.ToHexString(RandomNumberGenerator.GetBytes(5)),
            UserId = customer.UserId,
            TotalGuests = model.TotalGuests,
            EventLocation = model.EventLocation!.Trim(),
            EventDate = DateTime.SpecifyKind(model.EventDate!.Value.Date, DateTimeKind.Unspecified),
            TotalPrice = totalMenuPrice + operationFee,
            MenuItemsJson = JsonSerializer.Serialize(new { items = menu.Select(item => item.ItemName), pricePerPerson, operationFee, totalMenuPrice }),
            BookingStatus = "pending",
            CreatedDate = DateTime.UtcNow
        };
        customer.UserPhone = model.UserPhone!.Trim();
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Confirmation), new { bookingId = booking.BookingId });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(string bookingId)
    {
        if (await CurrentCustomer() is not { } customer) return RedirectToAction("Login", "Account");
        var booking = await db.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == customer.UserId);
        if (booking is null) return NotFound();
        ViewBag.UserInfo = customer;
        return View(booking);
    }

    [HttpGet]
    public async Task<IActionResult> MyBookings()
    {
        if (await CurrentCustomer() is not { } customer) return RedirectToAction("Login", "Account");
        return View(await db.Bookings.Where(b => b.UserId == customer.UserId).OrderByDescending(b => b.CreatedDate).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(string bookingId)
    {
        if (await CurrentCustomer() is not { } customer) return RedirectToAction("Login", "Account");
        var updated = await db.Bookings.Where(b => b.BookingId == bookingId && b.UserId == customer.UserId && b.BookingStatus == "pending")
            .ExecuteUpdateAsync(values => values.SetProperty(b => b.BookingStatus, "cancelled"));
        if (updated == 0) return NotFound();
        return RedirectToAction(nameof(MyBookings));
    }

    [HttpGet]
    public async Task<IActionResult> GetMenuItemsByCategory(string category) => Json(new
    {
        success = true,
        items = await db.MenuItems.Where(m => m.Category == category).OrderBy(m => m.ItemName)
            .Select(m => new { id = m.MenuItemId, name = m.ItemName, price = m.BasePrice }).ToListAsync()
    });

    [HttpPost]
    public async Task<IActionResult> CalculatePrice([FromBody] Dictionary<string, int> selectedItems)
    {
        if (selectedItems is null || selectedItems.Values.Any(quantity => quantity is < 1 or > 200)) return BadRequest();
        var ids = selectedItems.Keys.ToList();
        var items = await db.MenuItems.Where(m => ids.Contains(m.MenuItemId)).ToListAsync();
        if (items.Count != ids.Count || items.Any(item => !selectedItems.ContainsKey(item.MenuItemId))) return BadRequest();
        return Json(new { success = true, totalPrice = items.Sum(item => item.BasePrice * selectedItems[item.MenuItemId]) });
    }

    private async Task<Userinformation?> CurrentCustomer()
    {
        var id = HttpContext.Session.GetString("UserId");
        return id is null || HttpContext.Session.GetString("Role") != "customer" ? null : await db.Userinformations.FindAsync(id);
    }

    private async Task<IActionResult> ShowForm(BookingViewModel model)
    {
        var menu = await db.MenuItems.AsNoTracking().OrderBy(m => m.ItemName).ToListAsync();
        ViewBag.MenuItemsByCategory = menu.GroupBy(m => m.Category).ToDictionary(g => g.Key, g => g.ToList());
        ViewBag.FeeTiers = FeeTiers;
        return View("Create", model);
    }
}
