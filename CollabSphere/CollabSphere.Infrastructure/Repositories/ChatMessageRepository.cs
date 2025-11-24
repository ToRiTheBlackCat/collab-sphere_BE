using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Infrastructure.Repositories
{
    public class ChatMessageRepository : GenericRepository<ChatMessage>, IChatMessageRepository
    {
        public ChatMessageRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<ChatMessage>> GetChatConversationMessages(int conversationId)
        {
            var messages = await _context.ChatMessages
                .AsNoTracking()
                .Where(x => x.ConversationId == conversationId)
                .ToListAsync();

            return messages;
        }
    }
}
