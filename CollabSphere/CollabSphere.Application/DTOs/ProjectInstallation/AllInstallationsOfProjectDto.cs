using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.ProjectRepo
{
    public class AllInstallationsOfProjectDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public long GithubInstallationId { get; set; }
        public int TeamId { get; set; } 
        public int InstallatedByUserId { get; set; }
        public DateTime? InstalledAt { get; set; }
    }
}
