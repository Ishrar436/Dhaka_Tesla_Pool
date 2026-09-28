using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs
{
    public class PoolDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = null!;
        public string VehicleName { get; set; } = null!;
        public int Capacity { get; set; }
        public int SeatsTaken { get; set; }
        public List<PoolPassengerDto> Passengers { get; set; } = new();
    }

    public class PoolPassengerDto
    {
        public Guid RideRequestId { get; set; }
        public string PassengerName { get; set; } = null!;
        public int Seats { get; set; }
        public string PickupZoneName { get; set; } = null!;
        public string DropoffZoneName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public long FarePaisa { get; set; }
    }
}
