using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs
{
    public class CreateRideRequestDto
    {
        public Guid PassengerId { get; set; }
        public Guid PickupZoneId { get; set; }
        public Guid DropoffZoneId { get; set; }
        public int SeatsRequested { get; set; } = 1;
    }
}
