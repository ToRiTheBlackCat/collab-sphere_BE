using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.OTP
{
    public class OTPRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format!")]
        public string Email { get; set; } = string.Empty;
    }
}
