using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Commands.DenyProject
{
    public class DenyProjectCommand : ICommand
    {
        [FromRoute(Name = "ProjectId")]
        public int ProjectId { get; set; }
    }
}
