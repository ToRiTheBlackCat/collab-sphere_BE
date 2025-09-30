using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.User
{
    public class LoginResponseDTO
    {
        public bool IsAuthenticated { get; set; }
        public string? UserId { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
