using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Commands.CardCommands.UpdateCardDetails
{
    public class UpdateCardDetailsCommand
    {
        [JsonIgnore]
        public int WorkSpaceId { get; set; }
        [JsonIgnore]
        public int RequesterId { get; set; }
        [JsonIgnore]
        public int ListId { get; set; }
        [JsonIgnore]
        public int CardId { get; set; }

        public string? CardTitle { get; set; } 
        public string? CardDescription { get; set; }
        public string? RiskLevel { get; set; } 
        public DateTime? DueAt { get; set; }
        public bool IsComplete { get; set; }
        public List<CardAssignmentsRequest> AssignmentList { get; set; } = new List<CardAssignmentsRequest>();
    }
    public class CardAssignmentsRequest
    {
        [Required]
        public int StudentId { get; set; }
        [Required]
        public string StudentName { get; set; } = string.Empty;
    }
}
}
