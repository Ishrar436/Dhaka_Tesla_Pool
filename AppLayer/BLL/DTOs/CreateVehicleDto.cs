using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BLL.DTOs
{
    public class CreateVehicleDto
    {
        [Required, StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = null!;

        [Range(1, 8)]
        public int Capacity { get; set; }
    }
}
