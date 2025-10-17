using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Commands.UpdateClass
{
    public class UpdateClassCommand : ICommand
    {
        [Required]
        public UpdateClassDto ClassDto { get; set; }
    }
}
