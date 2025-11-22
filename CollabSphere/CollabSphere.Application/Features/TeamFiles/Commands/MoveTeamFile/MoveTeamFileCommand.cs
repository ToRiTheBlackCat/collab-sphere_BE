using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamFiles.Commands.MoveTeamFile
{
    public class MoveTeamFileCommand : ICommand
    {
        public int TeamId { get; set; }

        public int FileId { get; set; }

        public string FilePathPrefix { get; set; }

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1;
    }
}
