using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ChatConversations.Queries.GetUserConversations
{
    public class GetUserConversationsQuery : IQuery<GetUserConversationsResult>
    {
        public int? SemesterId { get; set; }

        public int? ClassId { get; set; }

        public int UserId = -1;

        public int UserRole = -1;
    }
}
