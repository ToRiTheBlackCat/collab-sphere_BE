using CollabSphere.Application.Base;
using CollabSphere.Application.Features.Subjects.Queries.GetAllSubject;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Admin.Queries
{
    public class AdminGetEmailsQuery : PaginationQuery, IQuery<AdminGetEmailsResult>
    {
    }
}
