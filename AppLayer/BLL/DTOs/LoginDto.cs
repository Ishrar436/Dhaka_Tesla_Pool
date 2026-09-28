using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs
{
    public class LoginDto
    {
        public string Phone { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
