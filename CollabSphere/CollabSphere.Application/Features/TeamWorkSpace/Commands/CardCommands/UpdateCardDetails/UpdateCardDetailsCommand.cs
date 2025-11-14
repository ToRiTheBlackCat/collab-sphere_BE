using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Commands.CardCommands.UpdateCardDetails
{
    public class UpdateCardDetailsCommand : ICommand
    {
        [JsonIgnore]
        public int WorkspaceId { get; set; }
        [JsonIgnore]
        public int RequesterId { get; set; }
        [JsonIgnore]
        public int ListId { get; set; }
        [JsonIgnore]
        public int CardId { get; set; }
        [Required]
        public string CardTitle { get; set; } = string.Empty;
        [Required]
        public string? CardDescription { get; set; } = string.Empty;
        [Required]
        public string? RiskLevel { get; set; } = string.Empty;
        public DateTime? DueAt { get; set; }
    }
}

