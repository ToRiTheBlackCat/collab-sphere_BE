using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWhiteboard.Commands.CreatePage
{
    public class CreatePageCommand : ICommand
    {
        [FromRoute(Name = "whiteboardId")]
        public int WhiteboardId { get; set; }

        [JsonIgnore]
        public int UserId = -1;

        [JsonIgnore]
        public int UserRole = -1;
        [Required]
        public string PageTitle { get; set; } = string.Empty;
    }
}
