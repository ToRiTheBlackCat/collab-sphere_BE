using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Features.Admin.Queries.AdminGetEmails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Admin.Queries.GetEmailDetail
{
    public class GetEmailDetailResult : QueryResult
    {
        public EmailDto? EmailDetail { get; set; }
    }
}
