using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Commands.TeamUploadAvatar
{
    public class TeamUploadAvatarCommand : ICommand
    {
        [FromForm(Name = "imageFile")]
        public IFormFile ImageFile { get; set; }

        [FromForm(Name = "teamId")]
        public int TeamId { get; set; }

        [FromForm(Name = "requesterId")]
        public int RequesterId { get; set; }
        [JsonIgnore]
        public string Folder = "team-avatars";
    }
}
