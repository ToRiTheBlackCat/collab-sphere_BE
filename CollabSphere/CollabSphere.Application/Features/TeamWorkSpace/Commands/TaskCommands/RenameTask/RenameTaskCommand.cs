using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Commands.TaskCommands.RenameTask
{
    public class RenameTaskCommand : ICommand
    {
        [JsonIgnore]
        public int WorkspaceId { get; set; }
        [JsonIgnore]
        public int ListId { get; set; }
        [JsonIgnore]
        public int RequesterId { get; set; }
        [JsonIgnore]
        public int CardId { get; set; }
        [JsonIgnore]
        public int TaskId { get; set; }
        [Required]
        public string NewTitle { get; set; } = string.Empty;
    }
}
