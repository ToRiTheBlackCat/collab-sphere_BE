using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.MilestoneEvaluations
{
    public class TeamMilestoneEvaluationVM
    {
        public int MilestoneId { get; set; }

        public int TeamId { get; set; }

        #region Lecturer Info
        public int LecturerId { get; set; }

        public string FullName { get; set; }

        public string AvatarImg { get; set; }

        public string LecturerCode { get; set; }

        public string PhoneNumber { get; set; }
        #endregion

        public decimal Score { get; set; }

        public string Comment { get; set; }

        public DateTime? CreatedDate { get; set; }

        public static explicit operator TeamMilestoneEvaluationVM(MilestoneEvaluation evaluation)
        {
            return new TeamMilestoneEvaluationVM()
            {
                MilestoneId = evaluation.MilestoneId,
                TeamId = evaluation.TeamId,

                LecturerId = evaluation.LecturerId,
                FullName = evaluation.Lecturer?.Fullname ?? "Lecturer relation missing",
                AvatarImg = evaluation.Lecturer?.AvatarImg ?? "Lecturer relation missing",
                PhoneNumber = evaluation.Lecturer?.PhoneNumber ?? "Lecturer relation missing",
                LecturerCode = evaluation.Lecturer?.LecturerCode ?? "Lecturer relation missing",

                Score = evaluation.Score ?? -1,
                Comment = evaluation.Comment,
                CreatedDate = evaluation.CreatedDate,
            };
        }
    }
}
