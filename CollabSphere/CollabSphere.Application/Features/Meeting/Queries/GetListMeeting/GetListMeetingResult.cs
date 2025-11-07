using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Teams;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Meeting.Queries.GetListMeeting
{
    public class GetListMeetingResult : QueryResult
    {
        public PagedList<Domain.Entities.Meeting>? PaginatedMeeting { get; set; }
    }
}
