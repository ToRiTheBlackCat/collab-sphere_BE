using CollabSphere.Application.DTOs.Checkpoints;
using CollabSphere.Domain.Entities;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Checkpoints
{
    public class CheckpointAssignmentVM
    {
        public int CheckpointAssignmentId { get; set; }

        public int CheckpointId { get; set; }

        //public int ClassId { get; set; }

        //public int TeamId { get; set; }

        #region Student Display Info
        public int ClassMemberId { get; set; }

        public int StudentId { get; set; }

        public string Fullname { get; set; }

        public string StudentCode { get; set; }

        public string AvatarImg { get; set; }
        #endregion

        public int? TeamRole { get; set; }

        public string TeamRoleString => this.TeamRole.HasValue && Enum.IsDefined(typeof(Application.Constants.TeamRole), this.TeamRole) ?
            ((Application.Constants.TeamRole)this.TeamRole).ToString() :
            $"Invalid team role ({this.TeamRole})";
    }
}

namespace CollabSphere.Application.Mappings.CheckpointAssginments
{
    public static partial class CheckpointAssginmentMappings
    {
        public static CheckpointAssignmentVM ToViewModel(this CheckpointAssignment assignment)
        {
            string fullName = "NOT FOUND";
            string avatarImg = "NOT FOUND";
            string studentCode = "NOT FOUND";
            int studentId = -1;
            int teamRole = -1;
            if (assignment.ClassMember != null)
            {
                studentId = assignment.ClassMember.StudentId;
                teamRole = assignment.ClassMember.TeamRole ?? -1;
                if (assignment.ClassMember.Student != null)
                {
                    fullName = assignment.ClassMember.Student.Fullname;
                    studentCode = assignment.ClassMember.Student.StudentCode;
                    avatarImg = assignment.ClassMember.Student.AvatarImg;
                }
            }

            return new CheckpointAssignmentVM()
            {
                CheckpointAssignmentId = assignment.CheckpointAssignmentId,
                CheckpointId = assignment.CheckpointId,

                ClassMemberId = assignment.ClassMemberId,
                StudentId = studentId,
                Fullname = fullName,
                StudentCode = studentCode,
                AvatarImg = avatarImg,
                TeamRole = teamRole,
            };
        }

        public static List<CheckpointAssignmentVM> ToViewModel(this IEnumerable<CheckpointAssignment> assignments)
        {
            return assignments.Select(x => x.ToViewModel()).ToList();
        }
    }
}
