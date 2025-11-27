using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Interfaces
{
    public interface IWhiteboardPageRepository : IGenericRepository<WhiteboardPage>
    {
        Task<List<WhiteboardPage>?> GetPagesOfWhiteboard(int whiteboardId);
        Task<WhiteboardPage?> GetWithShape(int pageId);
    }
}
