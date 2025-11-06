using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Meeting.Commands.CreateMeeting
{
    public class CreateMeetingCommand : ICommand
    {
        [JsonIgnore]
        public int UserId = -1;
        [JsonIgnore]
        public int UserRole = -1;

        [Required]
        public int TeamId { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public DateTime ScheduleTime { get; set; }
        [Required]
        public bool IsCallNow { get; set; }
    }
}
