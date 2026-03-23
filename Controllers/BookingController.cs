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
            // Get packages for dropdown
            var packages = await _context.Foodpackages.ToListAsync();
            ViewBag.Packages = new SelectList(packages, "PackageId", "PackageName");

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
            [Bind("PackageId,TotalGuests,EventLocation,EventDate")] BookingViewModel model)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError("", "กรุณาเข้าสู่ระบบก่อน");
                    return RedirectToAction("Login", "Account");
                }

                // Validate model
                if (!ModelState.IsValid)
                {
                    var packages = await _context.Foodpackages.ToListAsync();
                    ViewBag.Packages = new SelectList(packages, "PackageId", "PackageName");
                    return View(model);
                }

                // Get package price
                var package = await _context.Foodpackages.FindAsync(model.PackageId);
                if (package == null)
                {
                    ModelState.AddModelError("PackageId", "ไม่พบแพคเกจที่เลือก");
                    var packages = await _context.Foodpackages.ToListAsync();
                    ViewBag.Packages = new SelectList(packages, "PackageId", "PackageName");
                    return View(model);
                }

                // Calculate total price
                decimal pricePerPerson = package.PricePerPerson ?? 0;
                decimal totalPrice = pricePerPerson * (model.TotalGuests > 0 ? model.TotalGuests : 1);

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
                    PackageId = model.PackageId ?? "",
                    TotalGuests = model.TotalGuests,
                    EventLocation = model.EventLocation ?? "",
                    EventDate = model.EventDate,
                    TotalPrice = totalPrice,
                    BookingStatus = "pending"
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                return RedirectToAction("Confirmation", new { bookingId = booking.BookingId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"เกิดข้อผิดพลาด: {ex.Message}");
                var packages = await _context.Foodpackages.ToListAsync();
                ViewBag.Packages = new SelectList(packages, "PackageId", "PackageName");
                return View(model);
            }
        }

        // GET: Booking/Confirmation
        public async Task<IActionResult> Confirmation(string bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.PackageIdNavigation)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            var userInfo = await _context.Userinformations.FirstOrDefaultAsync(u => u.UserId == booking.UserId);
            ViewBag.UserInfo = userInfo;
            ViewBag.PackageName = booking.PackageIdNavigation?.PackageName ?? "Unknown";

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
                .Include(b => b.PackageIdNavigation)
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

        // API: Get package price
        [HttpGet]
        public async Task<IActionResult> GetPackagePrice(string packageId)
        {
            var package = await _context.Foodpackages.FindAsync(packageId);
            if (package == null)
            {
                return Json(new { success = false, message = "Package not found" });
            }

            return Json(new
            {
                success = true,
                packageName = package.PackageName,
                pricePerPerson = package.PricePerPerson ?? 0,
                minGuests = package.MinGuests ?? 2
            });
        }
    }

    
}
