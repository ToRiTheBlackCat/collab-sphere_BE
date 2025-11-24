using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Documents.Commands.CreateDocumentRoom
{
    public class CreateDocumentRoomResult : CommandResult
    {
        public int TeamId { get; set; }

        public string RoomName { get; set; }
    }
}
