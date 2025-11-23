using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Documents.Queries.GetTeamDocuments
{
    public class GetTeamDocumentsQuery : IQuery<GetTeamDocumentsResult>
    {
        public int TeamId { get; set; }

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1;
    }
}
