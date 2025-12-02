using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Student
{
    public class StudentSignUpRequestDto
    {
        [Required]
        public string OtpCode { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(5, ErrorMessage = "Password minimum length is 5")]
        public string Password { get; set; } = string.Empty;
        [Required]
        [MinLength(5, ErrorMessage = "Confirm Password minimum length is 5")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; } = string.Empty;
        [Required]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Phone number must start with 0 and be 10 digits.")]
        public string PhoneNumber { get; set; } = "0";

        public int? Yob { get; set; }
        [Required]
        public string School { get; set; } = string.Empty;
        [Required]
        public string StudentCode { get; set; } = string.Empty;
        [Required]
        public string Major { get; set; } = string.Empty;
    }
}
