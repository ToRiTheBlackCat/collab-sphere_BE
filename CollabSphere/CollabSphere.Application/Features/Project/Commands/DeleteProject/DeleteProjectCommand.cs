using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Commands.DeleteProject
{
    public class DeleteProjectCommand : ICommand
    {
        public int ProjectId { get; set; }

        /// <summary>
        /// UserId of the Viewer
        /// </summary>
        public int UserId = -1;

        /// <summary>
        /// Role of the Viewer
        /// </summary>
        public int UserRole = -1;
    }
}
