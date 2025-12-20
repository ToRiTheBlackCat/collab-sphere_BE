using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Project;
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

        public string BusinessRules { get; set; } = null!;

        public string Actors { get; set; } = null!;

        public string RejectReason { get; set; } = null!;

        public int Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int UpdatedBy { get; set; }

        public string StatusString => ((ProjectStatuses)Status).ToString();
    }
}

namespace CollabSphere.Application.Mappings.Projects
{
    public static partial class ProjectMappings
    {
        public static ProjectVM ToViewModel(this Project project)
        {
            return new ProjectVM()
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Description = project.Description,
                LecturerId = project.LecturerId,
                LecturerCode = project.Lecturer?.LecturerCode ?? "NOT_FOUND",
                LecturerName = project.Lecturer?.Fullname ?? "NOT_FOUND",
                SubjectId = project.SubjectId,
                SubjectCode = project.Subject?.SubjectCode ?? "NOT_FOUND",
                SubjectName = project.Subject?.SubjectName ?? "NOT_FOUND",
                BusinessRules = project.BusinessRules,
                Actors = project.Actors,
                RejectReason = project.RejectReason,
                Status = project.Status,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                UpdatedBy = project.UpdatedBy,
            };
        }

        public static List<ProjectVM> ToViewModels(this IEnumerable<Project> projects)
        {
            if (projects == null || !projects.Any())
            {
                return new List<ProjectVM>();
            }

            return projects.Select(x => x.ToViewModel()).ToList();
        }
    }
}
