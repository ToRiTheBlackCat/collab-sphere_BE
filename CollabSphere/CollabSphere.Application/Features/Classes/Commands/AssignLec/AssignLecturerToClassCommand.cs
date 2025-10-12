using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Commands.AssignLec
{
    public class AssignLecturerToClassCommand : ICommand
    {
        [FromRoute(Name = "classId")]
        public int ClassId { get; set; }
        [Required]
        public int LecturerId { get; set; }

        [JsonIgnore]
        public int UserId = -1;

        [JsonIgnore]
        public int UserRole = -1;
    }
}
