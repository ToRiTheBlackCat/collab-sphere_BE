using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Checkpoints
{
    public class CheckpointFileVM
    {
        public int FileId { get; set; }

        public int CheckpointId { get; set; }

        public string FilePath { get; set; }

        public string Type { get; set; }

        public static explicit operator CheckpointFileVM(CheckpointFile file)
        {
            return new CheckpointFileVM()
            {
                CheckpointId = file.CheckpointId,
                FilePath = file.FilePath,
                Type = file.Type,
                FileId = file.FileId,
            };
        }
    }
}
