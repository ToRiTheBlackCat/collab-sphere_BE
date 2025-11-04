using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.MilestoneReturns.Commands.DeleteMilestoneReturn
{
    public class DeleteMilestoneReturnCommand : ICommand
    {
        public int TeamMilestoneId { get; set; } = -1;

        public int MileReturnId { get; set; } = -1;

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1;
    }
}
