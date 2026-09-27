using System;
using System.Collections.Generic;

namespace DAL.EF.Tables;

public partial class Pool
{
    public Guid Id { get; set; }

    public Guid VehicleId { get; set; }

    public Guid DriverId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Driver Driver { get; set; } = null!;

    public virtual ICollection<RideRequest> RideRequests { get; set; } = new List<RideRequest>();

    public virtual Vehicle Vehicle { get; set; } = null!;
}
