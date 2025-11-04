using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Commands.LecEvaluateTeam
{
    public class LecturerEvaluateTeamCommand : ICommand
    {
        [JsonIgnore]
        public int TeamId { get; set; }
        [JsonIgnore]
        public int UserId = -1;
        [JsonIgnore]
        public int UserRole = -1;
        public string? TeamComment { get; set; }
        public List<EvaluateDetail> EvaluateDetails { get; set; } = new List<EvaluateDetail>();
    }
    public class EvaluateDetail
    {
        [Required]
        public int SubjectGradeComponentId { get; set; }
        [Required]
        [Range(0, 10)]
        public decimal Score { get; set; }
        public string? DetailComment { get; set; }
    }
}
