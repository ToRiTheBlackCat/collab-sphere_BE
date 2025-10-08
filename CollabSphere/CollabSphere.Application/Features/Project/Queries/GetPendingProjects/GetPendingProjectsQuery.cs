using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Queries.GetPendingProjects
{
    public class GetPendingProjectsQuery : PaginationQuery, IQuery<GetPendingProjectsResult>
    {
        public string Descriptors { get; set; } = string.Empty;
    }
}
