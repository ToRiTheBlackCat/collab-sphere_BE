using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.ClassMembers
{
    public class ClassMemberVM
    {
        public int ClassMemberId { get; set; }

        public int StudentId { get; set; }

        public string Fullname { get; set; }

        public string AvatarImg { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public string StudentCode { get; set; }

        public int? Yob { get; set; }

        public int? TeamId { get; set; }

        public string TeamName { get; set; }

        public int ClassId { get; set; }

        public int? TeamRole { get; set; }

        public bool IsGrouped { get; set; }

        public int Status { get; set; }

        public static explicit operator ClassMemberVM(ClassMember classMember)
        {
            var team = classMember.Team;
            var student = classMember.Student!;

            return new ClassMemberVM()
            {
                ClassMemberId = classMember.ClassMemberId,
                StudentId = classMember.StudentId,
                Fullname = student.Fullname,
                AvatarImg = student.AvatarImg,
                Address = student.Address,
                PhoneNumber = student.PhoneNumber,
                StudentCode = student.StudentCode,
                Yob = student.Yob,
                TeamId = classMember.TeamId,
                TeamName = team != null ? team.TeamName : string.Empty,
                ClassId = classMember.ClassId,
                TeamRole = classMember.TeamRole,
                IsGrouped = classMember.IsGrouped,
                Status = classMember.Status,
            };
        }
    }
}
