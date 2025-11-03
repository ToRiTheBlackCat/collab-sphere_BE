using CollabSphere.Application.DTOs.MilestoneFiles;
using CollabSphere.Application.DTOs.MilestoneReturns;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.MilestoneReturns
{
    public class TeamMilestoneReturnVM
    {
        public int MileReturnId { get; set; }

        public int TeamMilestoneId { get; set; }

        #region Student Info
        public int UserId { get; set; }

        public string StudentName { get; set; } = null!;

        public string StudentCode { get; set; } = null!;

        public string AvatarImg { get; set; } = null!;
        #endregion

        public string FileName { get; set; } = null!;

        public string Type { get; set; } = null!;

        public long FileSize { get; set; }

        public DateTime SubmitedDate { get; set; }

        public string FileUrl { get; set; } = null!;

        public DateTime UrlExpireTime { get; set; }
    }
}

namespace CollabSphere.Application.Mappings.MilestoneReturns
{
    public static partial class MilestoneReturnMappings
    {
        public static TeamMilestoneReturnVM ToViewModel(this MilestoneReturn milestoneReturn)
        {
            var fullName = "NOT FOUND";
            var avatarImg = "NOT FOUND";
            var studentCode = "NOT FOUND";
            if (milestoneReturn.User != null)
            {
                if (milestoneReturn.User.IsTeacher)
                {
                    throw new Exception("User must be a student");
                }

                fullName = milestoneReturn.User.Student?.Fullname ?? fullName;
                avatarImg = milestoneReturn.User.Student?.AvatarImg ?? avatarImg;
                studentCode = milestoneReturn.User.Student?.StudentCode ?? studentCode;
            }

            return new TeamMilestoneReturnVM()
            {
                MileReturnId = milestoneReturn.MileReturnId,
                TeamMilestoneId = milestoneReturn.TeamMilestoneId,
                UserId = milestoneReturn.UserId,
                StudentName = fullName,
                AvatarImg = avatarImg,
                StudentCode = studentCode,
                FileName = milestoneReturn.FileName,
                Type = milestoneReturn.Type,
                FileSize = milestoneReturn.FileSize,
                SubmitedDate = milestoneReturn.SubmitedDate,
                FileUrl = milestoneReturn.FileUrl,
                UrlExpireTime = milestoneReturn.UrlExpireTime,
            };
        }

        public static List<TeamMilestoneReturnVM> ToViewModel(this IEnumerable<MilestoneReturn> returns)
        {
            if (returns == null || !returns.Any())
            {
                return new List<TeamMilestoneReturnVM>();
            }

            return returns.Select(x => x.ToViewModel()).ToList();
        }
    }
}
