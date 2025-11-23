using CollabSphere.Application.DTOs.DocumentRooms;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.DocumentRooms
{
    public class DocumentRoomVM
    {
        public string RoomName { get; set; } = null!;

        public int TeamId { get; set; }

        public string TeamName { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}

namespace CollabSphere.Application.Mappings.DocumentRooms
{
    public static partial class DocumentRoomMappings
    {
        public static DocumentRoomVM ToViewModel(this DocumentRoom docRoom)
        {
            return new DocumentRoomVM()
            {
                RoomName = docRoom.RoomName,
                TeamId = docRoom.TeamId,
                TeamName = docRoom.Team?.TeamName ?? "NOT_FOUND",
                CreatedAt = docRoom.CreatedAt,
                UpdatedAt = docRoom.UpdatedAt,
            };
        }

        public static List<DocumentRoomVM> ToViewModels(this IEnumerable<DocumentRoom> docRooms)
        {
            if (docRooms == null || !docRooms.Any())
            {
                return new List<DocumentRoomVM>();
            }

            return docRooms.Select(x => x.ToViewModel()).ToList();
        }
    }
}