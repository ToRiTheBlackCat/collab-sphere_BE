using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Commands.WorkSpaceCommands.LeaveWorkspace
{
    public class LeaveWorkspaceCommand
    {
        public int WorkspaceId { get; set; }
        public int UserId { get; set; }
    }
}
