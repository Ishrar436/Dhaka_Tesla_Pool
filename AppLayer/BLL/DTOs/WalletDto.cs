using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs
{
    public class WalletDto
    {
        public Guid UserId { get; set; }
        public long BalancePaisa { get; set; }
    }
}
