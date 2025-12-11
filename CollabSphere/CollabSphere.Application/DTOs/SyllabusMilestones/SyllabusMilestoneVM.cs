using CollabSphere.Application.DTOs.SyllabusMilestones;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.SyllabusMilestones
{
    public class SyllabusMilestoneVM
    {
        public int SyllabusMilestoneId { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public int StartWeek { get; set; }

        public int Duration { get; set; }

        public int SyllabusId { get; set; }

        public int SubjectOutcomeId { get; set; }
    }
}

namespace CollabSphere.Application.Mappings.SyllabusMilestones
{
    public static class SyllabusMilestoneMappings
    {
        public static SyllabusMilestoneVM ToViewModel(this SyllabusMilestone syllabusMilestone)
        {
            return new SyllabusMilestoneVM()
            {
                SyllabusMilestoneId = syllabusMilestone.SyllabusMilestoneId,
                Title = syllabusMilestone.Title,
                Description = syllabusMilestone.Description,
                StartWeek = syllabusMilestone.StartWeek,
                Duration = syllabusMilestone.Duration,
                SyllabusId = syllabusMilestone.SyllabusId,
                SubjectOutcomeId = syllabusMilestone.SubjectOutcomeId,
            };
        }
        public static List<SyllabusMilestoneVM> ToViewModel(this IEnumerable<SyllabusMilestone> syllabusMilestones)
        {
            return syllabusMilestones.Select(x => x.ToViewModel()).ToList();
        }

    }
}
