using System;
using System.Collections.Generic;

namespace EventPack.Models.Db;

public partial class Booking
{
    public string BookingId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string? PackageId { get; set; }

    public DateTime? EventDate { get; set; }

    public string? EventLocation { get; set; }

    public int? TotalGuests { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? BookingStatus { get; set; }

    // Menu items selection stored as JSON
    public string? MenuItemsJson { get; set; }

    // Navigation properties
    public virtual Foodpackage? PackageIdNavigation { get; set; }
    public virtual Userinformation? UserIdNavigation { get; set; }
}
