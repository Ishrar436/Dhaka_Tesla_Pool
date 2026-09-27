using System;
using System.Collections.Generic;

namespace DAL.EF.Tables;

public partial class Wallet
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public long BalancePaisa { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User User { get; set; } = null!;
}
