using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Commands.EvaluateMileQuestionAns
{
    public class EvaluateMilestoneQuestionAnswerCommand : ICommand
    {
        [JsonIgnore]
        public int AnswerId { get; set; }
        [JsonIgnore]
        public int EvaluatorId = -1;
        [JsonIgnore]
        public int EvaluatorRole = -1;

        [Required]
        [Range(0, 5)]
        public int Score { get; set; } = 0;
        public string? Comment { get; set; }
    }
}
