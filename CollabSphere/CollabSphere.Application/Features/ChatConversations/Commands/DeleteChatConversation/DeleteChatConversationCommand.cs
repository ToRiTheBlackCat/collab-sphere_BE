using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ChatConversations.Commands.DeleteChatConversation
{
    public class DeleteChatConversationCommand : ICommand
    {
        public int ConversationId { get; set; }

        public int UserId { get; set; }

        public int UserRole { get; set; }
    }
}
