using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Commands.ImportClass
{
    public class ImportClassCommand : ICommand
    {
        public List<ImportClassDto> Classes { get; set; }
    }
}
