using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ChatConversations.Commands.MarkReadUserMessages
{
    public class MarkReadUserMessagesCommand : ICommand
    {
        [FromQuery(Name = "conversationId")]
        public int ConversationId { get; set; }

        public int UserId = -1;

        public int UserRole = -1;
    }
}
