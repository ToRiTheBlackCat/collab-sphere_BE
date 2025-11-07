using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.ProjectRepo
{
    public class AllReposOfProjectDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public long GithubRepoId { get; set; }
        public string? RepositoryName { get; set; }
        public string? RepositoryUrl { get; set; }
        public long? WebhookId { get; set; }
        public int ConnectedByUserId { get; set; }
        public DateTime? ConnectedAt { get; set; }
    }
}
