using System;
using System.Collections.Generic;

namespace DAL.EF.Tables;

public partial class Vehicle
{
    public Guid Id { get; set; }

    public Guid DriverId { get; set; }

    public string Name { get; set; } = null!;

    public int Capacity { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Driver Driver { get; set; } = null!;

    public virtual ICollection<Pool> Pools { get; set; } = new List<Pool>();
}
