using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Commands.CardCommands.CreateCardAndAssignMember
{
    public class CreateCardCommand : ICommand
    {
        [JsonIgnore]
        public int WorkspaceId { get; set; }
        [JsonIgnore]
        public int RequesterId { get; set; }
        [JsonIgnore]
        public int ListId { get; set; }

        [Required]
        public string CardTitle { get; set; } = string.Empty;
        public string? CardDescription { get; set; }
        [Required]
        public string RiskLevel { get; set; } = string.Empty;
        [Required]
        public float Position { get; set; }
        [JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueAt { get; set; }
        [Required]
        public bool IsComplete { get; set; } 
        [Required]
        public List<CardAssignmentsRequest> AssignmentList { get; set; } = new List<CardAssignmentsRequest>();
        public List<TaskOfCardRequest> TasksOfCard { get; set; } = new List<TaskOfCardRequest>();
    }
    public class CardAssignmentsRequest
    {
        [Required]
        public int StudentId { get; set; }
        [Required]
        public string StudentName { get; set; } = string.Empty;
    }
    public class TaskOfCardRequest
    {
        [Required]
        public string TaskTitle { get; set; } = string.Empty;
        [Required]
        public int TaskOrder { get; set; }
        public List<SubTaskOfCardRequest> SubTaskOfCard{ get; set; } = new List<SubTaskOfCardRequest>();
    }
    public class SubTaskOfCardRequest
    {
        [Required]
        public string SubTaskTitle { get; set; } = string.Empty;
        [Required]
        public int SubTaskOrder { get; set; }
        [Required]
        public bool IsDone { get; set; }
    }
}
