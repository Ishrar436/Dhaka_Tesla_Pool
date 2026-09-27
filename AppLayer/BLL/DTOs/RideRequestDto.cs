using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs
{
    public class RideRequestDto
    {
        public Guid Id { get; set; }
        public Guid PassengerId { get; set; }
        public string PickupZoneName { get; set; } = null!;
        public string DropoffZoneName { get; set; } = null!;
        public Guid? PoolId { get; set; }
        public string Status { get; set; } = null!;
        public long EstimatedFarePaisa { get; set; }
        public long? FinalFarePaisa { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}
