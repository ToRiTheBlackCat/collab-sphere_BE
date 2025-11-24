using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
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
                    .ThenInclude(msg => msg.MessageRecipients)
                .Include(x => x.ChatMessages)
                    .ThenInclude(msg => msg.Sender)
                        .ThenInclude(sender => sender.Student)
                .Include(x => x.ChatMessages)
                    .ThenInclude(msg => msg.Sender)
                        .ThenInclude(sender => sender.Lecturer)
                .Include(x => x.Team)
                    .ThenInclude(team => team.ClassMembers)
                        .ThenInclude(mem => mem.Student)
                            .ThenInclude(stu => stu.StudentNavigation)
                .Include(x => x.Team)
                    .ThenInclude(team => team.Class)
                        .ThenInclude(cls => cls.Lecturer)
                            .ThenInclude(lec => lec.LecturerNavigation)
                .FirstOrDefaultAsync(x => x.ConversationId == conversationId);

            if (conversation != null)
            {
                conversation.ChatMessages = conversation.ChatMessages.OrderBy(x => x.ConversationId).ToList();
            }

            return conversation;
        }


        public async Task<List<ChatConversation>> SeachConversations(int userId, int? teamId)
        {
            // Get teams that user is in
            var userTeams = await _context.Teams
                .AsNoTracking()
                .Include(x => x.Class)
                .Include(x => x.ClassMembers)
                .Include(x => x.ChatConversations)
                    .ThenInclude(x => x.ChatMessages)
                        .ThenInclude(msg => msg.Sender)
                            .ThenInclude(x => x.Lecturer)   
                .Include(x => x.ChatConversations)
                    .ThenInclude(x => x.ChatMessages)
                        .ThenInclude(msg => msg.Sender)
                            .ThenInclude(x => x.Student)
                .Include(x => x.ChatConversations)
                    .ThenInclude(x => x.ChatMessages)
                        .ThenInclude(msg => msg.MessageRecipients)
                .Where(x =>
                    (!teamId.HasValue || x.TeamId == teamId.Value) &&
                    (x.Class.LecturerId == userId || x.ClassMembers.Any(member => member.StudentId == userId))
                )
                .ToListAsync();

            // Get conversations in each team
            var conversations = userTeams.SelectMany(x => x.ChatConversations).ToList();

            return conversations;
        }

        public async Task<List<ChatConversation>> GetTeamConversation(int teamId)
        {
            var conversations = await _context.ChatConversations
                .Where(x => x.TeamId == teamId)
                .ToListAsync();

            return conversations;
        }
    }
}
