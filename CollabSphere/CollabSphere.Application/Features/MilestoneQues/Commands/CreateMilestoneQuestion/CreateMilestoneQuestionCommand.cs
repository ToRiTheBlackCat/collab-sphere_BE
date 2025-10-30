using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.MilestoneQuestions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.MilestoneQues.Commands.CreateMilestoneQuestion
{
    public class CreateMilestoneQuestionCommand : ICommand
    {
        [JsonIgnore]
        public int UserId = -1;
        [JsonIgnore]
        public int UserRole = -1;

        [JsonIgnore]
        public int TeamMilestoneId { get; set; }
        [Required]
        public int TeamId { get; set; }
        [Required]
        public string Question { get; set; } = string.Empty;
    }
}
