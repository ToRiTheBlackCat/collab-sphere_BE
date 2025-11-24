using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Documents.Commands.DeleteDocumentRoom
{
    public class DeleteDocumentRoomCommand : ICommand
    {
        public string RoomName { get; set; } = null!;

        public int TeamId { get; set; } = -1;

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1;
    }
}