using System;
using System.Collections.Generic;

namespace EventPack.Models.Db;

public partial class Staff
{
    public string StaffId { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Role { get; set; } = null!; // 'Chef', 'Sous Chef', 'Staff'

    public string? Phone { get; set; }
}
