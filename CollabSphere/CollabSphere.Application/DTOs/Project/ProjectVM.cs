using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Objective;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Project
{
    public class ProjectVM
    {
        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string Description { get; set; }

        #region Lecturer Info
        public int LecturerId { get; set; }

        public string LecturerCode { get; set; }

        public string LecturerName { get; set; }
        #endregion

        #region Subject Info
        public int SubjectId { get; set; }

        public string SubjectName { get; set; }

        public string SubjectCode { get; set; } 
        #endregion

        public int Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int UpdatedBy { get; set; }

        public string StatusString => ((ProjectStatuses)Status).ToString();

        public List<ObjectiveVM> Objectives { get; set; } = new List<ObjectiveVM>();

        public static explicit operator ProjectVM(Domain.Entities.Project project)
        {
            return new ProjectVM()
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Description = project.Description,
                LecturerId = project.LecturerId,
                LecturerCode = project.Lecturer?.LecturerCode ?? string.Empty,
                LecturerName = project.Lecturer?.Fullname ?? string.Empty,
                SubjectId = project.SubjectId,
                SubjectCode = project.Subject?.SubjectCode ?? string.Empty,
                SubjectName = project.Subject?.SubjectName ?? string.Empty,
                Objectives = 
                    project.Objectives?
                        .Select(x => (ObjectiveVM)x)
                        .OrderBy(x => x.ObjectiveMilestones.FirstOrDefault()?.StartDate)
                        .ToList() ??
                    new List<ObjectiveVM>(),
                Status = project.Status,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                UpdatedBy = project.UpdatedBy,
            };
        }
    }
}
