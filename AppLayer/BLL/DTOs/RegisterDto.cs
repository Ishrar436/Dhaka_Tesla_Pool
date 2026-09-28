using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs
{
    public class RegisterDto
    {
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = "Passenger"; // "Passenger" or "Driver"
    }
}
