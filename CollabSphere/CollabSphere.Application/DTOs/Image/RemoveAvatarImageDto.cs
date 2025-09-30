using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Image
{
    public class RemoveAvatarImageDto
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string PublicId { get; set; } = string.Empty;
    }
}
