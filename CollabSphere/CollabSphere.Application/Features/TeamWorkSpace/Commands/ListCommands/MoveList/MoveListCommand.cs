using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Commands.ListCommands.MoveList
{
    public class MoveListCommand
    {
        [JsonIgnore]
        public int WorkSpaceId { get; set; }
        [JsonIgnore]
        public int ListId { get; set; }
        [JsonIgnore]
        public int RequesterId { get; set; }
        [Required]
        public float NewPosition { get; set; }
    }
}
