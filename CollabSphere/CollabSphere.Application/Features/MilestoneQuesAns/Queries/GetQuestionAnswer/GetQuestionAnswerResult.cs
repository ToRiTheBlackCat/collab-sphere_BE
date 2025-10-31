using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.MilestoneQuestionAnswers;
using CollabSphere.Application.DTOs.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.MilestoneQuesAns.Queries.GetQuestionAnswer
{
    public class GetQuestionAnswerResult : QueryResult
    {
        public List<MilestoneQuestionAnswerDto>? AnswersList { get; set; }
    }
}
