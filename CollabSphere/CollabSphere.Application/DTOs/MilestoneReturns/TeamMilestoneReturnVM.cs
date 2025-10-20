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
        public int ClassMemberId { get; set; }

        public int StudentId { get; set; }

        public string Fullname { get; set; }

        public string AvatarImg { get; set; }

        public string StudentCode { get; set; }
        #endregion

        public string FilePath { get; set; }

        public string Type { get; set; }

        public DateTime SubmitedDate { get; set; }

        public static explicit operator TeamMilestoneReturnVM(MilestoneReturn milestoneReturn)
        {
            return new TeamMilestoneReturnVM()
            {
                MileReturnId = milestoneReturn.MileReturnId,
                TeamMilestoneId = milestoneReturn.TeamMilestoneId,

                ClassMemberId = milestoneReturn.ClassMemberId,
                StudentId = milestoneReturn.ClassMember?.Student?.StudentId ?? -1,
                Fullname = milestoneReturn.ClassMember?.Student?.Fullname ?? "Student relation missing",
                AvatarImg = milestoneReturn.ClassMember?.Student?.AvatarImg ?? "Student relation missing",
                StudentCode = milestoneReturn.ClassMember?.Student?.StudentCode ?? "Student relation missing",

                FilePath =milestoneReturn.FilePath,
                Type = milestoneReturn.Type,
                SubmitedDate = milestoneReturn.SubmitedDate,
            };
        }
    }
}
