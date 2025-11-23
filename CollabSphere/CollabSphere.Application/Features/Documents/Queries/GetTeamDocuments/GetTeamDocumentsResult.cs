using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.DocumentRooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Documents.Queries.GetTeamDocuments
{
    public class GetTeamDocumentsResult : QueryResult
    {
        public List<DocumentRoomVM> DocumentRooms { get; set; } = new List<DocumentRoomVM>();
    }
}
