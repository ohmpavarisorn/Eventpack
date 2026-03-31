using System;
using System.Collections.Generic;

namespace EventPack.Models.Db;

public partial class MenuItem
{
    public string MenuItemId { get; set; } = null!;

    public string ItemName { get; set; } = null!;

    public string Category { get; set; } = null!; // 'Appetizer', 'Main', 'Side', 'Dessert'

    public decimal BasePrice { get; set; }
}
