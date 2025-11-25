using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CollabSphere.Application.Features.GithubConnectionStates.Commands.GenerateNewInstallationUrl
{
    public class GenerateNewInstallationUrlCommand : ICommand<GenerateNewInstallationUrlResult>
    {
        public int ProjectId { get; set; } = -1;

        public int TeamId { get; set; } = -1;

        public int UserId = -1;

        public int UserRole = -1;
    }
}
