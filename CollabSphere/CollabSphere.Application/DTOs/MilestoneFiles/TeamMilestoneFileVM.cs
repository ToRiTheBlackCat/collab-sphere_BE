using CollabSphere.Application.DTOs.MilestoneFiles;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.MilestoneFiles
{
    public class TeamMilestoneFileVM
    {
        public int FileId { get; set; }

        public int TeamMilstoneId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = null!;

        public string AvatarImg { get; set; } = null!;

        public string FileName { get; set; } = null!;

        public string Type { get; set; } = null!;

        public long FileSize { get; set; }

        public DateTime CreatedAt { get; set; }

        public string FileUrl { get; set; } = null!;

        public DateTime UrlExpireTime { get; set; }
    }
}

namespace CollabSphere.Application.Mappings.MilestoneFiles
{
    public static class MilestoneFileMappings
    {
        public static TeamMilestoneFileVM ToViewModel(this MilestoneFile file)
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

            return new TeamMilestoneFileVM()
            {
                FileId = file.FileId,
                TeamMilstoneId = file.TeamMilstoneId,
                UserId = file.UserId,
                UserName = userName,
                AvatarImg = avatarImg,
                FileName = file.FileName,
                Type = file.Type,
                FileSize = file.FileSize,
                CreatedAt = file.CreatedAt,
                FileUrl = file.FileUrl,
                UrlExpireTime = file.UrlExpireTime,
            };
        }

        public static List<TeamMilestoneFileVM> ToViewModel(this IEnumerable<MilestoneFile> files)
        {
            if (files == null || !files.Any())
            {
                return new List<TeamMilestoneFileVM>();
            }

            return files.Select(x => x.ToViewModel()).ToList();
        }
    }
}
