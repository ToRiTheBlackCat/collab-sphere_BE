using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Queries.GetClassById
{
    public class GetClassByIdQuery : IQuery<GetClassByIdResult>
    {
        [FromRoute(Name = "classId")]
        public int ClassId { get; set; }

        public int ViewerUId;

        public int ViewerRole;
    }
}
