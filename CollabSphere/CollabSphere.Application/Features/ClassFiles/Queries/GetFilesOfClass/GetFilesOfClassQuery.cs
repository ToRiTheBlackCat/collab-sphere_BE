using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ClassFiles.Queries.GetFilesOfClass
{
    public class GetFilesOfClassQuery : IQuery<GetFilesOfClassResult>
    {
        public int ClassId { get; set; } = -1;

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1;
    }
}
