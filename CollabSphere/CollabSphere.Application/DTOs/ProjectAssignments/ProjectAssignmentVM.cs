using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.ProjectAssignments
{
    public class ProjectAssignmentVM
    {
        public int ProjectAssignmentId { get; set; }

        #region Project Info
        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string Description { get; set; }
        #endregion

        #region Class Info
        public int ClassId { get; set; }

        public string ClassName { get; set; } 
        #endregion

        public DateTime AssignedDate { get; set; }

        public static explicit operator ProjectAssignmentVM(ProjectAssignment projectAssignment)
        {
            return new ProjectAssignmentVM()
            {
                ProjectAssignmentId = projectAssignment.ProjectAssignmentId,
                ProjectId = projectAssignment.ProjectId,
                ProjectName = projectAssignment.Project.ProjectName,
                Description = projectAssignment.Project.Description,
                ClassId = projectAssignment.ClassId,
                ClassName = projectAssignment.Class.ClassName,
                AssignedDate = projectAssignment.AssignedDate,
            };
        }
    }
}
