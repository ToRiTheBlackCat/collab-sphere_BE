using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.TeamWorkspace;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Queries.GetCardDetailById
{
    public class GetCardDetailByIdResult : QueryResult
    {
        public Card? CardDetail { get; set; }
    }
}
