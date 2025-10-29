using CollabSphere.Application.DTOs.Checkpoints;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Checkpoints
{
    public class CheckpointFileVM
    {
        public required int FileId { get; set; }

        public required int CheckpointId { get; set; }

        public required int UserId { get; set; }

        public required string UserName { get; set; }

        public required string AvatarImg { get; set; }

        public required string FileName { get; set; }

        public required string Type { get; set; }

        public required string FilePath { get; set; }

        public required long FileSize { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required DateTime PathExpireTime { get; set; }
    }
}

namespace CollabSphere.Application.Mappings
{
    public static partial class CheckpointFileMappings
    {
        public static CheckpointFileVM ToViewModel(this CheckpointFile file)
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

            return new CheckpointFileVM()
            {
                FileId = file.FileId,
                CheckpointId = file.CheckpointId,
                UserId = file.UserId,
                UserName = userName,
                AvatarImg = avatarImg,
                FileName = file.FileName,
                FilePath = file.FilePath,
                FileSize = file.FileSize,
                Type = file.Type,
                CreatedAt = file.CreatedAt,
                PathExpireTime = file.PathExpireTime,
            };
        }

        public static List<CheckpointFileVM> ToViewModel(this IEnumerable<CheckpointFile> fileList)
        {
            return fileList.Select(x => x.ToViewModel()).ToList();
        }
    }
}
