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
    public class ChatConversationRepository : GenericRepository<ChatConversation>, IChatConversationRepository
    {
        public ChatConversationRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<ChatConversation?> GetConversationDetail(int conversationId)
        {
            var conversation = await _context.ChatConversations
                .AsNoTracking()
                .Include(x => x.ChatMessages)
                    .ThenInclude(x => x.Sender)
                        .ThenInclude(x => x.Student)
                .Include(x => x.ChatMessages)
                    .ThenInclude(x => x.Sender)
                        .ThenInclude(x => x.Lecturer)
                .FirstOrDefaultAsync(x => x.ConversationId == conversationId);

            if (conversation != null)
            {
                conversation.ChatMessages = conversation.ChatMessages.OrderBy(x => x.ConversationId).ToList();
            }

            return  conversation;
        }
    }
}
