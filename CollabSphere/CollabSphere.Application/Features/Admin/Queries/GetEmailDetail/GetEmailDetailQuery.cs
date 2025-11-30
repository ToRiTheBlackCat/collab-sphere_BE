using CollabSphere.Application.Base;
using CollabSphere.Application.Features.Admin.Queries.AdminGetEmails;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Admin.Queries.GetEmailDetail
{
    public class GetEmailDetailQuery: IQuery<GetEmailDetailResult>
    {
        [FromRoute(Name = "id")]
        public string Id { get; set; }
    }
}
