using System;
using System.Collections.Generic;

namespace DAL.EF.Tables;

public partial class User
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Driver? Driver { get; set; }

    public virtual ICollection<RideRequest> RideRequests { get; set; } = new List<RideRequest>();

    public virtual Wallet? Wallet { get; set; }
}
