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

    // Navigation properties
    public virtual Foodpackage? PackageIdNavigation { get; set; }
    public virtual Userinformation? UserIdNavigation { get; set; }
    public virtual ICollection<BookingMenuItem> BookingMenuItems { get; set; } = new List<BookingMenuItem>();
    public virtual ICollection<StaffAssignment> StaffAssignments { get; set; } = new List<StaffAssignment>();
}
