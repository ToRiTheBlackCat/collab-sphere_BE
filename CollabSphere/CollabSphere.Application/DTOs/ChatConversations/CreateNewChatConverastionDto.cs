using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.ChatConversations
{
    public class CreateNewChatConverastionDto
    {
        [Required]
        public int TeamId { get; set; }

        [Required]
        [Length(3, 100)]
        public string ConversationName { get; set; }
    }
}
