using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Commands.DeleteApprovedProject
{
    public class DeleteApprovedProjectCommand : ICommand
    {
        public int ProjectId = -1;

        public int UserId = -1;

        public int UserRole = -1;
    }
}
