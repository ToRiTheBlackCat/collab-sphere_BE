using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.CheckpointAssignments
{
    public class CheckpointAssignmentsDto
    {
        [FromRoute(Name = "checkpointId")]
        public int CheckpointId { get; set; }

        public HashSet<int> ClassMemberIds { get; set; } = new HashSet<int>();
    }
}
