using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Queries.GetClassById
{
    public class GetClassByIdResult : QueryResult
    {
        public bool Authorized { get; set; } = false;

        public ClassDetailDto? Class { get; set; }
    }
}
