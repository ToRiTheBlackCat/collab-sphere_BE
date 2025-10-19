using CollabSphere.Application.Base;
using CollabSphere.Application.Features.Classes.Commands.AddStudent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Commands.AddStudentsToTeam
{
    public class AddStudentToTeamCommand : ICommand
    {
        [Required]
        public int TeamId { get; set; }
        [JsonIgnore]
        public int UserId = -1;
        [JsonIgnore]
        public int UserRole = -1;
        public List<AddStudentToTeam> StudentList { get; set; } = new();
    }
    public class AddStudentToTeam
    {
        [Required]
        public int StudentId { get; set; }
        [Required]
        public int ClassId { get; set; }
    }
}
