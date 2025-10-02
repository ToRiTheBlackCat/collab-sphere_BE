using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Token
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
