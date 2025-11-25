using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.GithubConnectionStates.Commands.GenerateNewInstallationUrl
{
    public class GenerateNewInstallationUrlResult : CommandResult
    {
        public string StateToken { get; set; } = null!;

        public string GeneratedUrl { get; set; } = null!;

        //public string Jwt { get; set; } = null!;
    }
}
