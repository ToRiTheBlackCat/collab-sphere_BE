using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWhiteboard.Queries.GetShapeOfPage
{
    public class GetShapeOfPageQuery : IQuery<GetShapeOfPageResult>
    {
        [FromRoute(Name = "pageId")]
        public int PageId { get; set; }
    }
}
