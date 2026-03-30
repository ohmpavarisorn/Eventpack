using System;
using System.Collections.Generic;

namespace EventPack.Models.Db;

public partial class StaffAssignment
{
    public int AssignmentId { get; set; }

    public string BookingId { get; set; } = null!;

    public string StaffId { get; set; } = null!;

    public DateTime? AssignmentDate { get; set; }

    // Navigation properties
    public virtual Booking? Booking { get; set; }
    public virtual Staff? Staff { get; set; }
}
