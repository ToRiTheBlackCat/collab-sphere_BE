using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Checkpoints.Commands.CheckpointDeleteFile
{
    public class CheckpointDeleteFileCommand : ICommand
    {
        public int CheckpointId { get; set; } = -1;

        public int FileId { get; set; } = -1;

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1;
    }
}
