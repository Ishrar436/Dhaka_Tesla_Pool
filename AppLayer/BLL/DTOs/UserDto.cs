using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
