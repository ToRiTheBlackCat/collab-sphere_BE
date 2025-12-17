using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Semesters.Commands.DeleteSemester
{
    public class DeleteSemesterCommand : ICommand
    {
        [JsonIgnore]
        public int SemesterId { get; set; }
    }
}
