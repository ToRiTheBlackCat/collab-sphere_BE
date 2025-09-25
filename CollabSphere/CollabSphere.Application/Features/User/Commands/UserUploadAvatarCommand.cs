using CollabSphere.Application.DTOs.Student;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.User.Commands
{
    public class UserUploadAvatarCommand : IRequest<(bool, string)>
    {
        public IFormFile ImageFile { get; set; }
        public int UserId { get; set; }
        public bool IsTeacher { get; set; }
        public string FolderName { get; set; }
        public UserUploadAvatarCommand(IFormFile imageFile, int userId, bool isTeacher, string folderName)
        {
            ImageFile = imageFile;
            UserId = userId;
            IsTeacher = isTeacher;
            FolderName = folderName;
        }
    }
}
