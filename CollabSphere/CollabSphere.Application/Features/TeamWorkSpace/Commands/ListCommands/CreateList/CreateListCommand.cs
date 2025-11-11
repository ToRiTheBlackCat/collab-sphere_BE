using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Commands.ListCommands.CreateList
{
    public class CreateListCommand : ICommand
    {
        [JsonIgnore]
        public int WorkSpaceId { get; set; }
        [JsonIgnore]
        public int RequesterId { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public float Position { get; set; }
    }
}
