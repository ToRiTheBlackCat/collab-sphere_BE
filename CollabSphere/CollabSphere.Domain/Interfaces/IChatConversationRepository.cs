using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Intefaces
{
    public interface IChatConversationRepository : IGenericRepository<ChatConversation>
    {
        Task<ChatConversation?> GetConversationDetail(int conversationId);

        Task<List<ChatConversation>> SeachConversations(int userId, int? teamId);

        Task<List<ChatConversation>> GetTeamConversation(int teamId);
    }
}
