using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.MilestoneQuestionAnswers
{
    public class MilestoneQuestionAnswerDto
    {
        public int MilestoneQuestionAnsId { get; set; }
        public int MilestoneQuestionId { get; set; }
        public int ClassMemberId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? StudentCode { get; set; }
        public string? Answer { get; set; }
        public DateTime CreateTime { get; set; }
        public List<AnswerEvaluationDto> AnswerEvaluations { get; set; } = new List<AnswerEvaluationDto>();
    }
    public class AnswerEvaluationDto
    {
        public int AnswerEvaluationId { get; set; }
        public int EvaluatorId { get; set; }
        public string? EvaluatorName { get; set; }
        public decimal Score { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreateTime { get; set; }

    }
}
