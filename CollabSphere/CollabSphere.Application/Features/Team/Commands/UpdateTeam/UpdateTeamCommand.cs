using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace CollabSphere.Application.Features.Team.Commands.UpdateTeam
{
    public class UpdateTeamCommand : ICommand
    {
        [Required]
        public int TeamId { get; set; }

        [JsonIgnore]
        public int UserId = -1;

        [JsonIgnore]
        public int UserRole = -1;

        [Required]
        [Length(3, 100)]
        public string TeamName { get; set; } = string.Empty;

        public string? EnrolKey { get; set; }

        public string? Description { get; set; }

        public string? GitLink { get; set; }
    }
}
