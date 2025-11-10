using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ProjectRepo.Commands.CreateRepoForProject
{
    public class CreateInstallationForProjectCommand : ICommand
    {
        [JsonIgnore]
        public int ProjectId { get; set; }
        [JsonIgnore]
        public int UserId = -1;
        [JsonIgnore]
        public int UserRole = -1;
        [Required]
        [JsonPropertyName("installationId")]
        public long InstallationId { get; set; }
        [Required]
        [JsonPropertyName("teamId")]
        public int TeamId { get; set; }
    }
}
