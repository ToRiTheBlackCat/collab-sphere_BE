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
    public class DocumentRoomRepository : GenericRepository<DocumentRoom>, IDocumentRoomRepository
    {
        public DocumentRoomRepository(collab_sphereContext context) : base(context)
        {
        }

        public async Task<List<DocumentRoom>> GetDocRoomsByTeam(int teamId)
        {
            var rooms = await _context.DocRooms
                .AsNoTracking()
                .Include(x => x.Team)
                .Where(x => x.TeamId == teamId)
                .OrderBy(x => x.RoomName)
                    .ThenBy(x => x.CreatedAt)
                .ToListAsync();

            return rooms;
        }

        public async Task<DocumentRoom?> GetDocumentRoom(int teamId, string roomName)
        {
            var docRoom = await _context.DocRooms
                .AsNoTracking()
                .SingleOrDefaultAsync(x =>
                    x.RoomName.Equals(roomName) &&
                    x.TeamId == teamId
                );

            return docRoom;
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
    }
}
