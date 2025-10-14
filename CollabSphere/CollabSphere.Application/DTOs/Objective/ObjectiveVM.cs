using CollabSphere.Application.DTOs.ObjectiveMilestone;
using CollabSphere.Application.DTOs.SubjectModels;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Objective
{
    public class ObjectiveVM
    {
        public int ObjectiveId { get; set; }

        public string Description { get; set; }

        public string Priority { get; set; }

        public virtual List<ObjectiveMilestoneVM> ObjectiveMilestones { get; set; } = new List<ObjectiveMilestoneVM>();

        public static explicit operator ObjectiveVM(Domain.Entities.Objective objective)
        {
            return new ObjectiveVM()
            {
                ObjectiveId = objective.ObjectiveId,
                Description = objective.Description,
                Priority = objective.Priority,
                ObjectiveMilestones =
                    objective.ObjectiveMilestones?
                        .OrderBy(x => x.ObjectiveMilestoneId)
                        .Select(x => (ObjectiveMilestoneVM)x)
                        .OrderBy(x => x.StartDate)
                        .ToList() ??
                    new List<ObjectiveMilestoneVM>(),
            };
        }
    }
}
