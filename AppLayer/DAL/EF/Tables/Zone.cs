using System;
using System.Collections.Generic;

namespace DAL.EF.Tables;

public partial class Zone
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public virtual ICollection<RideRequest> RideRequestDropoffZones { get; set; } = new List<RideRequest>();

    public virtual ICollection<RideRequest> RideRequestPickupZones { get; set; } = new List<RideRequest>();
}
