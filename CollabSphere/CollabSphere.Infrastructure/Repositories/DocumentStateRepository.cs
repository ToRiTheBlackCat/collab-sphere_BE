using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
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
    public class DocumentStateRepository : GenericRepository<DocumentState>, IDocumentStateRepository
    {
        public DocumentStateRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<DocumentRoom?> GetDocumentRoomDetail(int teamId, string roomName)
        {
            var docRoom = await _context.DocRooms
                .AsNoTracking()
                .Include(x => x.DocumentStates)
                .Include(x => x.Team)
                    .ThenInclude(team => team.Class)
                .Include(x => x.Team)
                    .ThenInclude(team => team.ClassMembers)
                .SingleOrDefaultAsync(x => 
                    x.RoomName.Equals(roomName) &&
                    x.TeamId == teamId
                );

            return docRoom;
        }

        public async Task<List<DocumentState>> GetStatesByRoom(int teamId, string roomName)
        {
            var states = await _context.DocStates
                .AsNoTracking()
                .Where(x =>
                    x.RoomName.Equals(roomName) &&
                    x.TeamId == teamId
                )
                .OrderBy(x => x.CreatedTime)
                .ToListAsync();

            return states;
        }
    }
}
