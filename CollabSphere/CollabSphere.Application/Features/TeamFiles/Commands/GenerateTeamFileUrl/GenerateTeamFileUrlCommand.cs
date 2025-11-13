using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamFiles.Commands.GenerateTeamFileUrl
{
    public class GenerateTeamFileUrlCommand : ICommand<GenerateTeamFileUrlResult>
    {
        public int TeamId { get; set; } = -1;

        public int FileId { get; set; } = -1;

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1;
    }
}
