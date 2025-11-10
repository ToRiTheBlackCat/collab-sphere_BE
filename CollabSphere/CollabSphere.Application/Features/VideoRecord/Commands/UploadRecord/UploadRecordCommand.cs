using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.VideoRecord.Commands.UploadRecord
{
    public class UploadRecordCommand : ICommand
    {
        [FromForm]
        public IFormFile VideoFile { get; set; }
    }
}
