using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.DocumentRooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Documents.Commands.CreateDocumentRoom
{
    public class CreateDocumentRoomCommand : ICommand
    {
        public int TeamId { get; set; }

        public CreateDocumentRoomDto RoomDto { get; set; } = null!;

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1;
    }
}
