using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs
{
    public class ZoneDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
