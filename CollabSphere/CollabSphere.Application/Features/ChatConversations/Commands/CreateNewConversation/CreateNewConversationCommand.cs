using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.ChatConversations;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ChatConversations.Commands.CreateNewConversation
{
    public class CreateNewConversationCommand : ICommand<CreateNewConversationResult>
    {
        public CreateNewChatConverastionDto ChatConversation { get; set; } = null!;

        public int UserId = -1;

        public int UserRole = -1;
    }
}
