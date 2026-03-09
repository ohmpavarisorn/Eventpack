using System;
using System.Collections.Generic;

namespace EventPack.Models.Db;

public partial class Payment
{
    public string PaymentId { get; set; } = null!;

    public string BookingId { get; set; } = null!;

    public DateTime? PaymentDate { get; set; }

    public decimal? AmountPaid { get; set; }

    public string? PaymentMethod { get; set; }

    public string? EvidenceImage { get; set; }
}
