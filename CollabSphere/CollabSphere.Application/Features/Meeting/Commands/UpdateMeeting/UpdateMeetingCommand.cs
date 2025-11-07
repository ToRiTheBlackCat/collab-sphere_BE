using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Meeting.Commands.UpdateMeeting
{
    public class UpdateMeetingCommand : ICommand
    {
        [FromRoute(Name = "meetingId")]
        public int MeetingId { get; set; }
        [JsonIgnore]
        public int UserId = -1;
        [JsonIgnore]
        public int UserRole = -1;

        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? RecordUrl { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public int? Status { get; set; }
    }
}
