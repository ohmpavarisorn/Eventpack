using System.Collections.Generic;

public class BookingViewModel
{
    public int TotalGuests { get; set; }
    public string? EventLocation { get; set; }
    public string? UserPhone { get; set; }
    public DateTime? EventDate { get; set; }
    
    // Menu item selections: Dictionary<MenuItemId, Quantity>
    public Dictionary<string, int>? SelectedMenuItems { get; set; } = new();
    
    public decimal TotalPrice { get; set; }
}