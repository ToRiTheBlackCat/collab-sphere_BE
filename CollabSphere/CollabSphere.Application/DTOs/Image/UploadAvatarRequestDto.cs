using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Image
{
    public class UploadAvatarRequestDTO
    {
        [FromForm(Name = "imageFile")]
        public IFormFile File { get; set; }

        [FromForm(Name = "userId")]
        public int UserId { get; set; }
        [FromForm(Name = "isTeacher")]
        public bool IsTeacher { get; set; }
    }
}
