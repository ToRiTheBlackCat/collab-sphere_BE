using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Meeting.Queries.GetListMeeting
{
    public class GetListMeetingQuery : PaginationQuery, IQuery<GetListMeetingResult>
    {
        [JsonIgnore]
        public int UserId;
        [JsonIgnore]
        public int UserRole;

        [Required]
        public int TeamId { get; set; }
        public string? Title { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public int? Status { get; set; }
        public bool IsDesc { get; set; } = false;
    }
}
