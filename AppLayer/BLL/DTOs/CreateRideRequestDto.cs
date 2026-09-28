using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BLL.DTOs
{
    public class CreateRideRequestDto
    {
        [Required] public Guid PickupZoneId { get; set; }
        [Required] public Guid DropoffZoneId { get; set; }

        [Range(1, 8)]
        public int SeatsRequested { get; set; } = 1;
    }
}
