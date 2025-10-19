using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.MilestoneFiles
{
    public class TeamMilestoneFileVM
    {
        public int FileId { get; set; }

        public int TeamMilstoneId { get; set; }

        public string FilePath { get; set; }

        public string Type { get; set; }

        public static explicit operator TeamMilestoneFileVM(MilestoneFile file)
        {
            return new TeamMilestoneFileVM()
            {
                FileId = file.FileId,
                TeamMilstoneId = file.TeamMilstoneId,
                FilePath = file.FilePath,
                Type = file.Type,
            };
        }
    }
}
