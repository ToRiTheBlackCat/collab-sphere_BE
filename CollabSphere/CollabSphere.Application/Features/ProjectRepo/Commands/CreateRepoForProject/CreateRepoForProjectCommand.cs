using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ProjectRepo.Commands.CreateRepoForProject
{
    public class CreateRepoForProjectCommand : ICommand
    {
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public long GithubRepoId { get; set; }
        [Required]
        public string RepositoryName { get; set; } = string.Empty;
        [Required]
        public string RepositoryUrl { get; set; } = string.Empty;
        public long? WebhookId { get; set; }
        [Required]
        public int ConnectedByUserId { get; set; }
    }
}
