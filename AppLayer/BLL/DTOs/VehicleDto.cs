using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs
{
    public class VehicleDto
    {
        public Guid Id { get; set; }
        public Guid DriverId { get; set; }
        public string Name { get; set; } = null!;
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
    }
}
