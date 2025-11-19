using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Interfaces
{
    public interface IDocumentRoomRepository : IGenericRepository<DocumentRoom>
    {
        Task<List<DocumentRoom>> GetDocRoomsByTeam(int teamId);

        Task<DocumentRoom?> GetDocumentRoom(int teamId, string roomName);

        Task<DocumentRoom?> GetDocumentRoomDetail(int teamId, string roomName);
    }
}
