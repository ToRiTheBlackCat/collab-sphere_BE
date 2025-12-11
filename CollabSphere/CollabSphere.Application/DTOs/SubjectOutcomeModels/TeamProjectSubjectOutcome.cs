using CollabSphere.Application.DTOs.SubjectOutcomeModels;
using CollabSphere.Application.DTOs.TeamMilestones;
using CollabSphere.Application.Mappings.TeamMilestones;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.SubjectOutcomeModels
{
    public class TeamProjectSubjectOutcome
    {
        public int SubjectOutcomeId { get; set; }

        public int SyllabusId { get; set; }

        public string OutcomeDetail { get; set; } = null!;

        public List<TeamProjectMilestoneVM> TeamMilestones { get; set; } = new List<TeamProjectMilestoneVM>();
    }
}

namespace CollabSphere.Application.Mappings.SubjectOutcomes
{
    public static partial class SubjectOutcomeMappings
    {
        public static TeamProjectSubjectOutcome TeamProjectSubjectOutcome(this SubjectOutcome subjectOutcome)
        {
            return new TeamProjectSubjectOutcome()
            {
                SubjectOutcomeId = subjectOutcome.SubjectOutcomeId,
                SyllabusId = subjectOutcome.SyllabusId,
                OutcomeDetail = subjectOutcome.OutcomeDetail,
                TeamMilestones = subjectOutcome.SyllabusMilestones.Select(x => x.TeamMilestones.Single()).ToTeamProjectMilestoneVMs(),
            };
        }

        public static List<TeamProjectSubjectOutcome> TeamProjectSubjectOutcomes(this IEnumerable<SubjectOutcome> subjectOutcomes)
        {
            if (subjectOutcomes == null || !subjectOutcomes.Any())
            {
                return new List<TeamProjectSubjectOutcome>();
            }

            return subjectOutcomes.Select(x => x.TeamProjectSubjectOutcome()).ToList();
        }
    }
}
