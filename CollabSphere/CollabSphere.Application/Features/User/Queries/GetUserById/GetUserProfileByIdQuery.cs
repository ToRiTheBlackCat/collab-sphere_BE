using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.User.Queries.GetUserById
{
    public class GetUserProfileByIdQuery : IQuery<GetUserProfileByIdResult>
    {
        [FromRoute(Name = "userId")]
        public int UserId { get; set; }

        public int ViewerUId;

        public int ViewerRole;
    }
}
