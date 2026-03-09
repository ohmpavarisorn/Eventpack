using System;
using System.Collections.Generic;

namespace EventPack.Models.Db;

public partial class Foodpackage
{
    public string PackageId { get; set; } = null!;

    public string PackageName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? PricePerPerson { get; set; }

    public int? MinGuests { get; set; }
}
