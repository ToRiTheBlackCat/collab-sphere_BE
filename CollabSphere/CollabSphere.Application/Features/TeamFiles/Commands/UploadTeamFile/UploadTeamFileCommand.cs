using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamFiles.Commands.UploadTeamFile
{
    public class UploadTeamFileCommand : ICommand
    {
        public int TeamId { get; set; }

        public IFormFile File { get; set; }

        public string FilePathPrefix { get; set; } = "";

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1;
    }
}
