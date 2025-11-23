using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.ChatConversations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ChatConversations.Queries.GetUserConversations
{
    public class GetUserConversationsResult : QueryResult
    {
        public List<ChatConversationVM> ChatConversations { get; set; } = new List<ChatConversationVM>();
    }
}
