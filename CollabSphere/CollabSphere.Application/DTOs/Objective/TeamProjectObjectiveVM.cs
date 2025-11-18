using CollabSphere.Application.DTOs.Objective;
using CollabSphere.Application.DTOs.TeamMilestones;
using CollabSphere.Application.Mappings.TeamMilestones;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Objective
{
    public class TeamProjectObjectiveVM
    {
        public int ObjectiveId { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public List<TeamProjectMilestoneVM> TeamMilestones { get; set; } = new List<TeamProjectMilestoneVM>();
    }
}

namespace CollabSphere.Application.Mappings.Objectives
{
    public static partial class ObjectiveMappings
    {
        public static TeamProjectObjectiveVM ToTeamProjectobjectiveVM(this Objective objective)
        {
            return new TeamProjectObjectiveVM()
            {
                ObjectiveId = objective.ObjectiveId,
                Description = objective.Description,
                Priority = objective.Priority,
                TeamMilestones = objective.ObjectiveMilestones.Select(x => x.TeamMilestones.Single()).ToTeamProjectMilestoneVMs(),
            };
        }

        public static List<TeamProjectObjectiveVM> ToTeamProjectobjectiveVM(this IEnumerable<Objective> objectives)
        {
            if (objectives == null || !objectives.Any())
            {
                return new List<TeamProjectObjectiveVM>();
            }

            return objectives.Select(ojt => ojt.ToTeamProjectobjectiveVM()).ToList();
        }
    }
}
