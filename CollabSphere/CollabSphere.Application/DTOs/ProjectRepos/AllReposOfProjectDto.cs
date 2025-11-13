using CollabSphere.Application.Features.ProjectRepo.Commands.CreateRepoForProject;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.ProjectRepo
{
    public class AllReposOfProjectDto
    {
        public int TeamId { get; set; }
        public string? TeamName { get; set; }
        public List<RepoResponseDto> Repositories { get; set; } = new List<RepoResponseDto>();
    }

    public class RepoResponseDto
    {
        public string RepositoryFullName { get; set; } = string.Empty;
        public long RepositoryId { get; set; }
    }
}
