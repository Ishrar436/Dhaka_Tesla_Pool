using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BLL.DTOs
{
    public class RegisterDto
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = null!;

        [Required, RegularExpression(@"^\+?\d{10,15}$", ErrorMessage = "Phone must be 10 to 15 digits.")]
        public string Phone { get; set; } = null!;

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = null!;

        [RegularExpression("^(Passenger|Driver)$", ErrorMessage = "Role must be Passenger or Driver.")]
        public string Role { get; set; } = "Passenger";
    }
}
