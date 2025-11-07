using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Meeting.Queries.GetMeetingDetail
{
    public class GetMeetingDetailResult : QueryResult
    {
        public Domain.Entities.Meeting? FoundMeeting { get; set; }
    }
}
