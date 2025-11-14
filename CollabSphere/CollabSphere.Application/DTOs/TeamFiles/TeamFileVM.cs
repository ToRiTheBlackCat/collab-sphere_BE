using CollabSphere.Application.DTOs.ClassFiles;
using CollabSphere.Application.DTOs.TeamFiles;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.TeamFiles
{
    public class TeamFileVM
    {
        public int FileId { get; set; }

        public int TeamId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string AvatarImg { get; set; }

        public string FileName { get; set; }

        public string FilePathPrefix { get; set; }

        public string Type { get; set; }

        public long FileSize { get; set; }

        public DateTime CreatedAt { get; set; }

        public string FileUrl { get; set; }

        public DateTime UrlExpireTime { get; set; }
    }
}

namespace CollabSphere.Application.Mappings.TeamFiles
{
    public static partial class TeamFileMappings
    {
        public static TeamFileVM ToViewModel(this TeamFile teamFile)
        {
            var userName = "NOT FOUND";
            var avatarImg = "NOT FOUND";

            if (teamFile.User != null)
            {
                userName = teamFile.User.IsTeacher ?
                    teamFile.User.Lecturer?.Fullname ?? userName :
                    teamFile.User.Student?.Fullname ?? userName;
                avatarImg = teamFile.User.IsTeacher ?
                    teamFile.User.Lecturer?.AvatarImg ?? avatarImg :
                    teamFile.User.Student?.AvatarImg ?? avatarImg;
            }

            return new TeamFileVM()
            {
                FileId = teamFile.FileId,
                TeamId = teamFile.TeamId,
                UserId = teamFile.UserId,
                UserName = userName,
                AvatarImg = avatarImg,
                FileName = teamFile.FileName,
                FilePathPrefix = teamFile.FilePathPrefix,
                Type = teamFile.Type,
                FileSize = teamFile.FileSize,
                CreatedAt = teamFile.CreatedAt,
                FileUrl = teamFile.FileUrl,
                UrlExpireTime = teamFile.UrlExpireTime,
            };
        }

        public static List<TeamFileVM> ToViewModel(this IEnumerable<TeamFile> teamFiles)
        {
            if (teamFiles == null || !teamFiles.Any())
            {
                return new List<TeamFileVM>();
            }

            return teamFiles.Select(teamFile => teamFile.ToViewModel()).ToList();
        }
    }
}
