using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventPack.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
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
            // Try to get menu items from database, if table doesn't exist return empty
            Dictionary<string, List<MenuItem>>? menuItems = null;
            try
            {
                menuItems = await _context.MenuItems
                    .GroupBy(m => m.Category)
                    .ToDictionaryAsync(g => g.Key, g => g.ToList());
                Console.WriteLine($"✓ MenuItems Count: {menuItems?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                // MenuItems table doesn't exist yet, use empty
                Console.WriteLine($"✗ Error loading menuitems: {ex.Message}");
                menuItems = new Dictionary<string, List<MenuItem>>();
            }

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
        public async Task<IActionResult> Create(
            [Bind("TotalGuests,EventLocation,EventDate,UserPhone")] BookingViewModel model)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                Console.WriteLine($"📝 BookingCreate POST - UserId: {userId}");
                
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("❌ No UserId in session");
                    ModelState.AddModelError("", "กรุณาเข้าสู่ระบบก่อน");
                    return RedirectToAction("Login", "Account");
                }

                // Get selected menu items from checkboxes
                var selectedMenuIds = new List<string>();
                var formKeys = HttpContext.Request.Form.Keys;
                
                foreach (var key in formKeys)
                {
                    if (key.StartsWith("menu_") && !key.StartsWith("menu_qty_"))
                    {
                        var menuItemId = key.Replace("menu_", "");
                        selectedMenuIds.Add(menuItemId);
                    }
                }

                // Validate that at least one menu item is selected
                if (selectedMenuIds.Count == 0)
                {
                    Console.WriteLine("❌ No menu items selected");
                    ModelState.AddModelError("", "กรุณาเลือกเมนูอาหารอย่างน้อยหนึ่งรายการ");
                    Dictionary<string, List<MenuItem>>? menuItems = null;
                    try
                    {
                        menuItems = await _context.MenuItems
                            .GroupBy(m => m.Category)
                            .ToDictionaryAsync(g => g.Key, g => g.ToList());
                    }
                    catch { menuItems = new Dictionary<string, List<MenuItem>>(); }
                    ViewBag.MenuItemsByCategory = menuItems;
                    return View(model);
                }

                Console.WriteLine($"📋 Selected menu items: {string.Join(", ", selectedMenuIds)}");

                // Validate total guests
                if (model.TotalGuests < 2)
                {
                    Console.WriteLine($"❌ TotalGuests validation failed: {model.TotalGuests}");
                    ModelState.AddModelError("TotalGuests", "จำนวนคนต้องมากกว่า 1 คน");
                    Dictionary<string, List<MenuItem>>? menuItems = null;
                    try
                    {
                        menuItems = await _context.MenuItems
                            .GroupBy(m => m.Category)
                            .ToDictionaryAsync(g => g.Key, g => g.ToList());
                    }
                    catch { menuItems = new Dictionary<string, List<MenuItem>>(); }
                    ViewBag.MenuItemsByCategory = menuItems;
                    return View(model);
                }

                Console.WriteLine($"✓ TotalGuests: {model.TotalGuests}");

                // Calculate price per person from selected items
                decimal pricePerPerson = 0;
                var menuItemDetails = new Dictionary<string, decimal>();
                
                try
                {
                    foreach (var menuItemId in selectedMenuIds)
                    {
                        var menuItem = await _context.MenuItems.FindAsync(menuItemId);
                        if (menuItem != null)
                        {
                            pricePerPerson += menuItem.BasePrice;
                            menuItemDetails[menuItemId] = menuItem.BasePrice;
                        }
                    }
                }
                catch
                {
                    // MenuItems table doesn't exist, use default pricing
                    pricePerPerson = 100 * selectedMenuIds.Count;
                }

                // Calculate total price: (price per person × guests) + operation fee
                decimal totalMenuPrice = pricePerPerson * model.TotalGuests;
                decimal operationFee = GetOperationFee(model.TotalGuests);
                decimal totalPrice = totalMenuPrice + operationFee;

                // Update user info if provided
                var userInfo = await _context.Userinformations.FirstOrDefaultAsync(u => u.UserId == userId);
                if (userInfo != null)
                {
                    if (!string.IsNullOrEmpty(model.UserPhone))
                        userInfo.UserPhone = model.UserPhone;
                    if (!string.IsNullOrEmpty(model.EventLocation))
                        userInfo.UserAddress = model.EventLocation;
                }

                // Create menu items JSON
                var menuItemsJson = JsonSerializer.Serialize(new
                {
                    items = selectedMenuIds,
                    pricePerPerson = pricePerPerson,
                    operationFee = operationFee,
                    totalMenuPrice = totalMenuPrice
                });

                // Create booking
                var booking = new Booking
                {
                    BookingId = GenerateBookingId(),
                    UserId = userId,
                    TotalGuests = model.TotalGuests,
                    EventLocation = model.EventLocation ?? "",
                    EventDate = model.EventDate,
                    TotalPrice = totalPrice,
                    MenuItemsJson = menuItemsJson,
                    BookingStatus = "pending",
                    CreatedDate = DateTime.Now
                };

                Console.WriteLine($"📋 Booking object created:");
                Console.WriteLine($"  - BookingId: {booking.BookingId}");
                Console.WriteLine($"  - UserId: {booking.UserId}");
                Console.WriteLine($"  - TotalGuests: {booking.TotalGuests}");
                Console.WriteLine($"  - EventLocation: {booking.EventLocation}");
                Console.WriteLine($"  - TotalPrice: {booking.TotalPrice}");
                Console.WriteLine($"  - EventDate: {booking.EventDate}");
                Console.WriteLine($"  - MenuItemsJson: {booking.MenuItemsJson}");

                _context.Bookings.Add(booking);
                
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"❌ DbUpdateException: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"❌ Inner: {dbEx.InnerException.Message}");
                    }
                    throw;
                }
                
                Console.WriteLine($"✅ Booking saved successfully: {booking.BookingId}");
                return RedirectToAction("Confirmation", new { bookingId = booking.BookingId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in Create: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                
                // Log inner exceptions
                var innerEx = ex.InnerException;
                int level = 1;
                while (innerEx != null)
                {
                    Console.WriteLine($"Inner Exception {level}: {innerEx.Message}");
                    Console.WriteLine($"Inner Stack {level}: {innerEx.StackTrace}");
                    innerEx = innerEx.InnerException;
                    level++;
                }
                
                string errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg = $"{ex.Message} - {ex.InnerException.Message}";
                }
                
                ModelState.AddModelError("", $"เกิดข้อผิดพลาด: {errorMsg}");
                Dictionary<string, List<MenuItem>>? menuItems = null;
                try
                {
                    menuItems = await _context.MenuItems
                        .GroupBy(m => m.Category)
                        .ToDictionaryAsync(g => g.Key, g => g.ToList());
                }
                catch { menuItems = new Dictionary<string, List<MenuItem>>(); }
                ViewBag.MenuItemsByCategory = menuItems;
                return View(model);
            }
        }

        // GET: Booking/Confirmation
        public async Task<IActionResult> Confirmation(string bookingId)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return RedirectToAction("Create");
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
            try
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
            catch
            {
                return Json(new { success = false, items = new List<object>() });
            }
        }

        // API: Calculate total price
        [HttpPost]
        public async Task<IActionResult> CalculatePrice([FromBody] Dictionary<string, int> selectedItems)
        {
            try
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
            catch
            {
                return Json(new { success = false, totalPrice = 0 });
            }
        }

        // Helper method to calculate operation fee based on guest count
        private decimal GetOperationFee(int guestCount)
        {
            return guestCount switch
            {
                >= 2 and <= 10 => 5000,
                >= 11 and <= 20 => 9000,
                >= 21 and <= 35 => 12000,
                >= 36 and <= 50 => 18000,
                >= 60 and <= 80 => 35000,
                >= 81 => 50000,
                _ => 0
            };
        }
    }
}
