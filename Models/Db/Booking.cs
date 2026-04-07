using System;
using System.Collections.Generic;

namespace EventPack.Models.Db;

public partial class Booking
{
    public string BookingId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public DateTime? EventDate { get; set; }

    public string? EventLocation { get; set; }

    public int? TotalGuests { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? BookingStatus { get; set; }

    public string? MenuItemsJson { get; set; }

    public DateTime? CreatedDate { get; set; }

    // Navigation properties
    public virtual Userinformation? UserIdNavigation { get; set; }
}
