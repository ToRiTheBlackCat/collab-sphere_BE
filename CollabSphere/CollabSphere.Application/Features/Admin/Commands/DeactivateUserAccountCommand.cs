using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Admin.Commands
{
    public class DeactivateUserAccountCommand : ICommand
    {
        [FromRoute(Name = "userId")]
        public int UserId { get; set; }
    }
}
