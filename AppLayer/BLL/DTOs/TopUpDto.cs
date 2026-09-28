using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BLL.DTOs
{
    public class TopUpDto
    {
        [Range(1, 100_000_000)]
        public long AmountPaisa { get; set; }
    }
}
