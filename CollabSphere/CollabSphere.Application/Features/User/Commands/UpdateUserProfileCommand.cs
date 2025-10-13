using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace CollabSphere.Application.Features.User.Commands
{
    public class UpdateUserProfileCommand : ICommand, IValidatableObject
    {
        [Required]
        public int UserId { get; set; }

        [JsonIgnore]
        public int RequesterId = -1;

        [JsonIgnore]
        public int RequesterRole = -1;

        [Required]
        public bool IsTeacher { get; set; }

        #region Change password
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }
        #endregion

        #region Profile info
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150, ErrorMessage = "Address cannot exceed 150 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Phone number must start with 0 and be 10 digits.")]
        public string PhoneNumber { get; set; } = "0";

        public int? Yob { get; set; } = 0;

        [Required]
        public string School { get; set; } = string.Empty;

        [Required]
        [MaxLength(30, ErrorMessage = "Code cannot exceed 30 characters.")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(50, ErrorMessage = "Major cannot exceed 50 characters.")]
        public string Major { get; set; } = string.Empty;
        #endregion
        public bool IsActive { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // Check if user wants to change password
            bool isPasswordChangeRequested =
                !string.IsNullOrWhiteSpace(OldPassword) ||
                !string.IsNullOrWhiteSpace(NewPassword) ||
                !string.IsNullOrWhiteSpace(ConfirmNewPassword);

            if (isPasswordChangeRequested)
            {
                // Require all fields if any password field is provided
                if (string.IsNullOrWhiteSpace(OldPassword))
                    results.Add(new ValidationResult("Old password is required when changing password.", new[] { nameof(OldPassword) }));

                if (string.IsNullOrWhiteSpace(NewPassword))
                    results.Add(new ValidationResult("New password is required when changing password.", new[] { nameof(NewPassword) }));

                if (string.IsNullOrWhiteSpace(ConfirmNewPassword))
                    results.Add(new ValidationResult("Please confirm your new password.", new[] { nameof(ConfirmNewPassword) }));

                // Validate match between new and confirm password
                if (!string.IsNullOrWhiteSpace(NewPassword) &&
                    !string.IsNullOrWhiteSpace(ConfirmNewPassword) &&
                    !NewPassword.Equals(ConfirmNewPassword))
                {
                    results.Add(new ValidationResult("New password and confirmation do not match.", new[] { nameof(NewPassword), nameof(ConfirmNewPassword) }));
                }

                // Password strength check 
                if (!string.IsNullOrWhiteSpace(NewPassword) && NewPassword.Length < 5)
                {
                    results.Add(new ValidationResult("New password must be at least 5 characters long.", new[] { nameof(NewPassword) }));
                }
            }

            return results;
        }
    }
}
