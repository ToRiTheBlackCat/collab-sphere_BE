using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.GithubConnectionStates.Commands.MapReposFromInstallation
{
    public class MapReposFromInstallationCommand : ICommand<MapReposFromInstallationResult>
    {
        [FromQuery(Name = "installation_id")]
        public long InstallationId { get; set; }

        [FromQuery(Name = "state")]
        public string StateToken { get; set; } = null!;
    }
}
