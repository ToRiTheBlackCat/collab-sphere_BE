using CollabSphere.Application.DTOs.Objective;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.ObjectiveMilestone
{
    public class ObjectiveMilestoneVM
    {
        public int ObjectiveMilestoneId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        //public int ObjectiveId { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public static explicit operator ObjectiveMilestoneVM(Domain.Entities.ObjectiveMilestone objectiveMilestone)
        {
            return new ObjectiveMilestoneVM()
            {
                ObjectiveMilestoneId = objectiveMilestone.ObjectiveMilestoneId,
                Title = objectiveMilestone.Title,
                Description = objectiveMilestone.Description,
                //ObjectiveId = objectiveMilestone.ObjectiveId,
                StartDate = objectiveMilestone.StartDate,
                EndDate = objectiveMilestone.EndDate,
            };
        }
    }
}
