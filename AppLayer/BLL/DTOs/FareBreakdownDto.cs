using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs
{
    public class FareBreakdownDto
    {
        public long BaseFarePaisa { get; set; }
        public long DistanceChargePaisa { get; set; }
        public long PoolDiscountPaisa { get; set; }
        public long TotalFarePaisa { get; set; }
    }
}
