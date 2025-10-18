using CollabSphere.Domain.Entities;
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

        public int ClassId { get; set; }

        public int TeamId { get; set; }

        #region Student Display Info
        public int ClassMemberId { get; set; }

        public int StudentId { get; set; }

        public string Fullname { get; set; }

        public string StudentCode { get; set; }

        public string AvatarImg { get; set; }
        #endregion

        public int TeamRole { get; set; }

        public static explicit operator CheckpointAssignmentVM(CheckpointAssignment assignment)
        {
            return new CheckpointAssignmentVM()
            {
                CheckpointAssignmentId = assignment.CheckpointAssignmentId,
                CheckpointId = assignment.CheckpointId,
                ClassId = assignment.ClassMember.ClassId,
                TeamId = assignment.ClassMember.TeamId!.Value,

                ClassMemberId = assignment.ClassMemberId,
                StudentId = assignment.ClassMember.StudentId,
                Fullname = assignment.ClassMember.Fullname,
                StudentCode = assignment.ClassMember.Student.StudentCode,
                AvatarImg = assignment.ClassMember.Student.AvatarImg,
                TeamRole = assignment.ClassMember.TeamRole!.Value,
            };
        }
    }
}
