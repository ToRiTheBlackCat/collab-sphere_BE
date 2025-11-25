using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CollabSphere.Application.Common
{
    public static class ControllerBaseExtention
    {
        public struct JwtUserInfo
        {
            public int UserId { get; set; }

            public int RoleId { get; set; }
        }

        public static JwtUserInfo GetCurrentUserInfo(this ControllerBase controller)
        {
            var UIdClaim = controller.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = controller.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            JwtUserInfo userInfo = new JwtUserInfo()
            {
                UserId = int.TryParse(UIdClaim?.Value, out int userId) ? userId : -1,
                RoleId = int.TryParse(roleClaim?.Value, out int roleId) ? roleId : -1,
            };

            return userInfo;
        }
    }
}
