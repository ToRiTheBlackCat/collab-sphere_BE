using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.MilestoneQuesAns.Commands.UpdateQuestionAnswer
{
    public class UpdateQuestionAnswerCommand : ICommand
    {
        [JsonIgnore]
        public int UserId = -1;
        [JsonIgnore]
        public int UserRole = -1;
        [JsonIgnore]
        public int QuestionId { get; set; }
        [JsonIgnore]
        public int AnswerId { get; set; }
        [Required]
        public string Answer { get; set; } = string.Empty;
    }
}
