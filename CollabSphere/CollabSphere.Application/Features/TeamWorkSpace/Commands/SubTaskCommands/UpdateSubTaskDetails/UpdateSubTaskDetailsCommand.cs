using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Commands.SubTaskCommands.UpdateSubTaskDetails
{
    public class UpdateSubTaskDetailsCommand
    {
        [JsonIgnore]
        public int WorkSpaceId { get; set; }
        [JsonIgnore]
        public int ListId { get; set; }
        [JsonIgnore]
        public int RequesterId { get; set; }
        [JsonIgnore]
        public int CardId { get; set; }
        [JsonIgnore]
        public int TaskId { get; set; }
        [JsonIgnore]
        public int SubTaskId { get; set; }

        public string? SubTaskTitle { get; set; }
        public bool IsDone { get; set; }
    }
}
