using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWhiteboard.Commands.UpdatePageTitle
{
    public class UpdatePageTitleCommand : ICommand
    {
        [FromRoute(Name = "pageId")]
        public int PageId { get; set; }
        [Required]
        public string NewPageTitle { get; set; } = string.Empty;
    }
}
