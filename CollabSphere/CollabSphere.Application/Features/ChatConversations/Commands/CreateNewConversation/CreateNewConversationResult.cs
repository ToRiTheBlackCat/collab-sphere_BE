using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ChatConversations.Commands.CreateNewConversation
{
    public class CreateNewConversationResult : CommandResult
    {
        public int ConversationId { get; set; }
    }
}
