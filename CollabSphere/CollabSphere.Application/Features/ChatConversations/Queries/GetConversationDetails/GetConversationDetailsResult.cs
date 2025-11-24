using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.ChatConversations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ChatConversations.Queries.GetConversationDetails
{
    public class GetConversationDetailsResult : QueryResult
    {
        public ChatConversationDetailDto? ChatConversation { get; set; }
    }
}
