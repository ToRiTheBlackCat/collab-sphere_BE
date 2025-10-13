using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.User.Queries.GetUserById
{
    public class GetUserProfileByIdResult : QueryResult
    {
        public bool Authorized { get; set; } = false;

        public UserProfileDto? User { get; set; }
    }
}
