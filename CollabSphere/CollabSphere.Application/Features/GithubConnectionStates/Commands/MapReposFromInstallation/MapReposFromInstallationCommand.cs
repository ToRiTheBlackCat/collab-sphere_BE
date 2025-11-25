using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.GithubConnectionStates.Commands.MapReposFromInstallation
{
    public class MapReposFromInstallationCommand : ICommand<MapReposFromInstallationResult>
    {
        public long InstallationId { get; set; }

        public string StateToken { get; set; } = null!;

        public int UserId = -1;

        public int UserRole = -1;
    }
}
