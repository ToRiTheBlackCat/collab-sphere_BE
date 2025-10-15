using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Commands.AddStudent
{
    public class AddStudentToClassCommand : ICommand
    {
        [Required]
        public int ClassId { get; set; }
        [JsonIgnore]
        public int UserId = -1;
        [JsonIgnore]
        public int UserRole = -1;
        public List<AddStudentToClass> StudentList { get; set; } = new();
    }

    public class AddStudentToClass
    {
        [Required]
        public int StudentId { get; set; }
        [Required]
        public string StudentName { get; set; } = string.Empty;
    }
}
