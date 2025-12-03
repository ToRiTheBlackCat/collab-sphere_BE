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
                .Include(x => x.Users)
                    .ThenInclude(user => user.Student)
                        .ThenInclude(stu => stu.ClassMembers)
                            .ThenInclude(member => member.Team)
                .Include(x => x.Users)
                    .ThenInclude(user => user.Lecturer)
                .Include(x => x.Team)
                    .ThenInclude(team => team.Class)
                .FirstOrDefaultAsync(x => x.ConversationId == conversationId);

            if (conversation != null)
            {
                conversation.ChatMessages = conversation.ChatMessages.OrderBy(x => x.ConversationId).ToList();

                // Only get students' class member reference of the class
                foreach (var user in conversation.Users.Where(x => !x.IsTeacher))
                {
                    user.Student.ClassMembers = user.Student.ClassMembers.Where(x => x.ClassId == conversation.ClassId).ToList();
                }
            }

            return conversation;
        }


        public async Task<List<ChatConversation>> GetConversationsByUser(int userId, int? semesterId, int? classId)
        {
            var conversations = await _context.ChatConversations
                .AsNoTracking()
                .Include(x => x.Users)
                .Include(x => x.Class)
                    .ThenInclude(cls => cls.Semester)
                .Include(x => x.Team)
                .Include(x => x.LatestMessage)
                    .ThenInclude(x => x.Sender)
                .Include(x => x.ChatMessages)
                    .ThenInclude(x => x.Sender)
                .Where(x =>
                    x.Users.Any(x => x.UId == userId) &&
                    (!semesterId.HasValue || x.Class.SemesterId == semesterId) &&
                    (!classId.HasValue || classId == x.ClassId)
                )
                .ToListAsync();

            if (conversations.Any())
            {
                conversations = conversations.OrderByDescending(x => x.Class.Semester.StartDate).ToList();
            }

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
