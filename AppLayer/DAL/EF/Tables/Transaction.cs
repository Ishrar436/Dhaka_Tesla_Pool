using System;
using System.Collections.Generic;

namespace DAL.EF.Tables;

public partial class Transaction
{
    public Guid Id { get; set; }

    public Guid WalletId { get; set; }

    public Guid? RideRequestId { get; set; }

    public long AmountPaisa { get; set; }

    public string Type { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual RideRequest? RideRequest { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
}
