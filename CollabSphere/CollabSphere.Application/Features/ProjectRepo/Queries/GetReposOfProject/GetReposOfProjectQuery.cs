using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ProjectRepo.Queries.GetReposOfProject
{
    public class GetReposOfProjectQuery : PaginationQuery, IQuery<GetReposOfProjectResult>
    {
        [FromRoute(Name = "projectId")]
        public int ProjectId { get; set; }

        [FromQuery]
        public string? RepositoryName { get; set; }
        [FromQuery]
        public string? RepositoryUrl { get; set; }
        [FromQuery]
        public int ConnectedUserId { get; set; } = 0;
        [FromQuery]
        public DateTime? FromDate { get; set; }
        [FromQuery]
        public bool IsDesc { get; set; } = false;
    }
}

