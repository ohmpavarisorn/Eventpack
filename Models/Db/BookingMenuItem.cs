using System;
using System.Collections.Generic;

namespace EventPack.Models.Db;

public partial class BookingMenuItem
{
    public int BookingMenuItemId { get; set; }

    public string BookingId { get; set; } = null!;

    public string MenuItemId { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    // Navigation properties
    public virtual Booking? Booking { get; set; }
    public virtual MenuItem? MenuItem { get; set; }
}
