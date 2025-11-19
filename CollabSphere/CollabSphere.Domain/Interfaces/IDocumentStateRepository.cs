using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Interfaces
{
    public interface IDocumentStateRepository : IGenericRepository<DocumentState>
    {
        Task<DocumentRoom?> GetDocumentRoomDetail(int teamId, string roomName);

        Task<List<DocumentState>> GetStatesByRoom(int teamId, string roomName);
    }
}
