using CollabSphere.Application.Base;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWhiteboard.Queries.GetShapeOfPage
{
    public class GetShapeOfPageResult : QueryResult
    {
        public List<Shape>? Shapes { get; set; } = new List<Shape>();
    }
}
