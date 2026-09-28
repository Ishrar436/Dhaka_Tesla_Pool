using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs
{
    public class DriverDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;
        public string LicenseNo { get; set; } = null!;
        public bool IsOnline { get; set; }
    }
}
