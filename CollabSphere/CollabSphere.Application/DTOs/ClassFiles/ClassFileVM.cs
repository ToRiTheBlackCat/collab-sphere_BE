using CollabSphere.Application.DTOs.ClassFiles;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.ClassFiles
{
    public class ClassFileVM
    {
        public int FileId { get; set; }

        public int ClassId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = null!;

        public string AvatarImg { get; set; } = null!;

        public string FileName { get; set; } = null!;

        public string Type { get; set; } = null!;

        public long FileSize { get; set; }

        public DateTime CreatedAt { get; set; }

        public string ObjectKey { get; set; } = null!;

        public string FileUrl { get; set; } = null!;

        public DateTime UrlExpireTime { get; set; }
    }
}

namespace CollabSphere.Application.Mappings.ClassFiles
{
    public static class ClassFileMappings
    {
        public static ClassFileVM ToViewModel(this ClassFile file)
        {
            var userName = "NOT FOUND";
            var avatarImg = "NOT FOUND";

            if (file.User != null)
            {
                userName = file.User.IsTeacher ?
                    file.User.Lecturer?.Fullname ?? userName :
                    file.User.Student?.Fullname ?? userName;
                avatarImg = file.User.IsTeacher ?
                    file.User.Lecturer?.AvatarImg ?? avatarImg :
                    file.User.Student?.AvatarImg ?? avatarImg;
            }

            return new ClassFileVM()
            {
                FileId = file.FileId,
                ClassId = file.ClassId,
                UserId = file.UserId,
                UserName = userName,
                AvatarImg = avatarImg,
                FileName = file.FileName,
                Type = file.Type,
                FileSize = file.FileSize,
                CreatedAt = file.CreatedAt,
                ObjectKey = file.ObjectKey,
                FileUrl = file.FileUrl,
                UrlExpireTime = file.UrlExpireTime,
            };
        }

        public static List<ClassFileVM> ToViewModel(this IEnumerable<ClassFile> files)
        {
            return files.Select(x => x.ToViewModel()).ToList();
        }
    }
}
