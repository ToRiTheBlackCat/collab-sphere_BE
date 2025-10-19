using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.MilestoneQuestions
{
    public class TeamMilestoneQuestionDto
    {
        public int MilestoneQuestionId { get; set; }

        public int TeamMilestoneId { get; set; }

        public int TeamId { get; set; }

        public string Question { get; set; }

        public int? AnswerCount { get; set; }

        public DateTime CreatedTime { get; set; }

        public static explicit operator TeamMilestoneQuestionDto(MilestoneQuestion milestoneQuestion)
        {
            return new TeamMilestoneQuestionDto()
            {
                MilestoneQuestionId = milestoneQuestion.MilestoneQuestionId,
                TeamMilestoneId = milestoneQuestion.TeamMilestoneId,
                TeamId = milestoneQuestion.TeamId,
                Question = milestoneQuestion.Question,
                AnswerCount = milestoneQuestion.AnswerCount ?? null,
                CreatedTime = milestoneQuestion.CreatedTime,
            };
        }
    }

}
