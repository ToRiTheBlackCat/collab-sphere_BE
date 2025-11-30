using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Admin.Commands.TrashEmail
{
    public class TrashEmailCommand : ICommand
    {
        [FromRoute(Name = "id")]
        public string Id { get; set; } 
    }
}
