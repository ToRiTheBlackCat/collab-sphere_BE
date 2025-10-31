using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.MilestoneFiles.Commands.UploadMilestoneFile
{
    public class UploadMilestoneFileCommand : ICommand
    {
        public int TeamMilestoneId { get; set; } = -1;

        public IFormFile File { get; set; } = null!;

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1;
    }
}
