using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs
{
    public class CreateVehicleDto
    {
        public string Name { get; set; } = null!;
        public int Capacity { get; set; }
    }
}
