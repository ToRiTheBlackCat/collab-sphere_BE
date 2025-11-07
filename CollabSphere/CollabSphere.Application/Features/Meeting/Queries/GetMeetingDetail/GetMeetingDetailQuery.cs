using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Meeting.Queries.GetMeetingDetail
{
    public class GetMeetingDetailQuery : IQuery<GetMeetingDetailResult>
    {
        [JsonIgnore]
        public int UserId;
        [JsonIgnore]
        public int UserRole;

        [FromRoute(Name = "meetingId")]
        public int MeetingId { get; set; }
    }
}
