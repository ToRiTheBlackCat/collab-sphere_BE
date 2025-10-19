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

        public DateTime CreatedDate { get; set; }

        public static explicit operator TeamMilestoneEvaluationVM(MilestoneEvaluation evaluation)
        {
            return new TeamMilestoneEvaluationVM()
            {
                MilestoneId = evaluation.MilestoneId,
                TeamId = evaluation.TeamId,

                LecturerId = evaluation.LecturerId,
                FullName = evaluation.Lecturer.Fullname,
                AvatarImg = evaluation.Lecturer.AvatarImg,
                PhoneNumber = evaluation.Lecturer.PhoneNumber,
                LecturerCode = evaluation.Lecturer.LecturerCode,

                Score = evaluation.Score!.Value,
                Comment = evaluation.Comment,
                CreatedDate = evaluation.CreatedDate!.Value,
            };
        }
    }
}
