using System;
using System.Collections.Generic;

namespace DAL.EF.Tables;

public partial class RideStatusHistory
{
    public Guid Id { get; set; }

    public Guid RideRequestId { get; set; }

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = null!;

    public DateTime ChangedAt { get; set; }

    public virtual RideRequest RideRequest { get; set; } = null!;
}
