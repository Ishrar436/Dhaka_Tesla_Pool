using System;
using System.Collections.Generic;

namespace DAL.EF.Tables;

public partial class RideRequest
{
    public Guid Id { get; set; }

    public Guid PassengerId { get; set; }

    public Guid PickupZoneId { get; set; }

    public Guid DropoffZoneId { get; set; }

    public Guid? PoolId { get; set; }

    public int SeatsRequested { get; set; }

    public string Status { get; set; } = null!;

    public long EstimatedFarePaisa { get; set; }

    public long? FinalFarePaisa { get; set; }

    public DateTime RequestedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual Zone DropoffZone { get; set; } = null!;

    public virtual User Passenger { get; set; } = null!;

    public virtual Zone PickupZone { get; set; } = null!;

    public virtual Pool? Pool { get; set; }

    public virtual ICollection<RideStatusHistory> RideStatusHistories { get; set; } = new List<RideStatusHistory>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
