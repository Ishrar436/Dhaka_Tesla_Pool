using System;
using System.Collections.Generic;

namespace DAL.EF.Tables;

public partial class Driver
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string LicenseNo { get; set; } = null!;

    public bool IsOnline { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Pool> Pools { get; set; } = new List<Pool>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
