using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
using Google.Apis.Drive.v3.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Infrastructure.Repositories
{
    public class MessageRecipientRepository : GenericRepository<MessageRecipient>, IMessageRecipientRepository
    {
        public MessageRecipientRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<MessageRecipient>> GetMessageRecipientInConversation(int userId, int conversationId)
        {
            var messageRecipients = await _context.MessageRecipients
                .AsNoTracking()
                .Include(x => x.Message)
                .Where(x =>
                    x.Message.ConversationId == conversationId &&
                    x.ReceiverId == userId
                )
                .ToListAsync();

            return messageRecipients;
        }

        public async Task<List<MessageRecipient>> GetRecipientsOfMessage(int messageId)
        {
            var messageRecipients = await _context.MessageRecipients
                .AsNoTracking()
                .Where(x =>
                    x.MessageId == messageId
                )
                .ToListAsync();

            return messageRecipients;
        }
    }
}
