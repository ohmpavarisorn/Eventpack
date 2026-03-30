using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventPack.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventPack.ViewModels;
namespace EventPack.Controllers
{
    public class BookingController : Controller
    {
        private readonly Csi402projectContext _context;

        public BookingController(Csi402projectContext context)
        {
            _context = context;
        }

        // GET: Booking/Create
        public async Task<IActionResult> Create()
        {
            // Get menu items grouped by category
            var menuItems = await _context.MenuItems
                .GroupBy(m => m.Category)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());
            
            ViewBag.MenuItemsByCategory = menuItems;

            // Get current user info if available
            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                var userInfo = await _context.Userinformations.FirstOrDefaultAsync(u => u.UserId == userId);
                if (userInfo != null)
                {
                    ViewBag.UserPhone = userInfo.UserPhone;
                    ViewBag.UserAddress = userInfo.UserAddress;
                    ViewBag.FullName = userInfo.FullName;
                    ViewBag.UserId = userId;
                }
            }

            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TotalGuests,EventLocation,EventDate,UserPhone")] BookingViewModel model)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError("", "กรุณาเข้าสู่ระบบก่อน");
                    return RedirectToAction("Login", "Account");
                }

                // Get selected menu items from form
                var selectedMenuItems = new Dictionary<string, int>();
                var formKeys = HttpContext.Request.Form.Keys;
                
                foreach (var key in formKeys)
                {
                    if (key.StartsWith("menu_qty_"))
                    {
                        var menuItemId = key.Replace("menu_qty_", "");
                        if (int.TryParse(HttpContext.Request.Form[key], out int quantity) && quantity > 0)
                        {
                            selectedMenuItems[menuItemId] = quantity;
                        }
                    }
                }

                // Validate that at least one menu item is selected
                if (selectedMenuItems.Count == 0)
                {
                    ModelState.AddModelError("", "กรุณาเลือกเมนูอาหารอย่างน้อยหนึ่งรายการ");
                    var menuItems = await _context.MenuItems
                        .GroupBy(m => m.Category)
                        .ToDictionaryAsync(g => g.Key, g => g.ToList());
                    ViewBag.MenuItemsByCategory = menuItems;
                    return View(model);
                }

                // Validate total guests
                if (model.TotalGuests < 2)
                {
                    ModelState.AddModelError("TotalGuests", "จำนวนคนต้องมากกว่า 1 คน");
                    var menuItems = await _context.MenuItems
                        .GroupBy(m => m.Category)
                        .ToDictionaryAsync(g => g.Key, g => g.ToList());
                    ViewBag.MenuItemsByCategory = menuItems;
                    return View(model);
                }

                // Calculate total price
                decimal totalPrice = 0;
                foreach (var item in selectedMenuItems)
                {
                    var menuItem = await _context.MenuItems.FindAsync(item.Key);
                    if (menuItem != null)
                    {
                        totalPrice += menuItem.BasePrice * item.Value;
                    }
                }

                // Update user info if provided
                var userInfo = await _context.Userinformations.FirstOrDefaultAsync(u => u.UserId == userId);
                if (userInfo != null)
                {
                    if (!string.IsNullOrEmpty(model.UserPhone))
                        userInfo.UserPhone = model.UserPhone;
                    if (!string.IsNullOrEmpty(model.EventLocation))
                        userInfo.UserAddress = model.EventLocation;
                }

                // Create booking
                var booking = new Booking
                {
                    BookingId = GenerateBookingId(),
                    UserId = userId,
                    PackageId = null,
                    TotalGuests = model.TotalGuests,
                    EventLocation = model.EventLocation ?? "",
                    EventDate = model.EventDate,
                    TotalPrice = totalPrice,
                    BookingStatus = "pending"
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Create BookingMenuItem records
                foreach (var item in selectedMenuItems)
                {
                    var menuItem = await _context.MenuItems.FindAsync(item.Key);
                    if (menuItem != null)
                    {
                        var bookingMenuItem = new BookingMenuItem
                        {
                            BookingId = booking.BookingId,
                            MenuItemId = item.Key,
                            Quantity = item.Value,
                            UnitPrice = menuItem.BasePrice,
                            TotalPrice = menuItem.BasePrice * item.Value
                        };
                        _context.BookingMenuItems.Add(bookingMenuItem);
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Confirmation", new { bookingId = booking.BookingId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"เกิดข้อผิดพลาด: {ex.Message}");
                var menuItems = await _context.MenuItems
                    .GroupBy(m => m.Category)
                    .ToDictionaryAsync(g => g.Key, g => g.ToList());
                ViewBag.MenuItemsByCategory = menuItems;
                return View(model);
            }
        }

        // GET: Booking/Confirmation
        public async Task<IActionResult> Confirmation(string bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingMenuItems)
                .ThenInclude(bm => bm.MenuItem)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            var userInfo = await _context.Userinformations.FirstOrDefaultAsync(u => u.UserId == booking.UserId);
            ViewBag.UserInfo = userInfo;

            return View(booking);
        }

        // GET: Booking/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = await _context.Bookings
                .Include(b => b.BookingMenuItems)
                .ThenInclude(bm => bm.MenuItem)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            return View(bookings);
        }

        private string GenerateBookingId()
        {
            // Format: BK + timestamp + random
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(100, 999);
            return $"BK{timestamp}{random}".Substring(0, 15);
        }

        // API: Get menu items by category
        [HttpGet]
        public async Task<IActionResult> GetMenuItemsByCategory(string category)
        {
            var items = await _context.MenuItems
                .Where(m => m.Category == category)
                .OrderBy(m => m.ItemName)
                .ToListAsync();

            return Json(new
            {
                success = true,
                items = items.Select(i => new
                {
                    id = i.MenuItemId,
                    name = i.ItemName,
                    price = i.BasePrice
                })
            });
        }

        // API: Calculate total price
        [HttpPost]
        public async Task<IActionResult> CalculatePrice([FromBody] Dictionary<string, int> selectedItems)
        {
            decimal totalPrice = 0;
            
            foreach (var item in selectedItems)
            {
                var menuItem = await _context.MenuItems.FindAsync(item.Key);
                if (menuItem != null)
                {
                    totalPrice += menuItem.BasePrice * item.Value;
                }
            }

            return Json(new
            {
                success = true,
                totalPrice = totalPrice
            });
        }
    }
}
