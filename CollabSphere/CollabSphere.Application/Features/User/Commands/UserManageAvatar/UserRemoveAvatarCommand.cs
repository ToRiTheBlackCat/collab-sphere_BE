using CollabSphere.Application.DTOs.Image;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.User.Commands.UserManageAvatar
{
    public class UserRemoveAvatarCommand : IRequest<(bool, string)>
    {
        public RemoveAvatarImageDto Dto { get; set; }

        public UserRemoveAvatarCommand(RemoveAvatarImageDto dto)
        {
            Dto = dto;
        }
    }
}
