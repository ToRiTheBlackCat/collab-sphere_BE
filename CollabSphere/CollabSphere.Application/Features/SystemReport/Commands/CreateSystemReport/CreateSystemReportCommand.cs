using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CollabSphere.Application.Features.SystemReport.Commands.CreateSystemReport
{
    public class CreateSystemReportCommand : ICommand
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        [FromForm(Name = "Attachments")]
        public List<IFormFile>? Attachments { get; set; }
    }
}
