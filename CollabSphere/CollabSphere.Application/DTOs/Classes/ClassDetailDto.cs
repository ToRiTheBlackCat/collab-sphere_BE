﻿using CollabSphere.Application.DTOs.ClassFiles;
using CollabSphere.Application.DTOs.ClassMembers;
using CollabSphere.Application.DTOs.ProjectAssignments;
using CollabSphere.Application.DTOs.Teams;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Classes
{
    public class ClassDetailDto
    {
        public int ClassId { get; set; }

        public string ClassName { get; set; }

        #region Subject Info
        public int SubjectId { get; set; }

        public string SubjectCode { get; set; }

        public string SubjectName { get; set; }
        #endregion

        #region Lecturer Info
        public int LecturerId { get; set; }

        public string LecturerCode { get; set; }

        public string LecturerName { get; set; }
        #endregion

        public string EnrolKey { get; set; }

        public int? MemberCount { get; set; }

        public int? TeamCount { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }

        public List<ClassFileVM> ClassFiles { get; set; } = new List<ClassFileVM>();

        public List<ClassMemberVM> ClassMembers { get; set; } = new List<ClassMemberVM>();

        public List<ProjectAssignmentVM> ProjectAssignments { get; set; } = new List<ProjectAssignmentVM>();

        public List<TeamVM> Teams { get; set; } = new List<TeamVM>();

        public static explicit operator ClassDetailDto(Class classEntity)
        {
            return new ClassDetailDto()
            {
                ClassId = classEntity.ClassId,
                ClassName = classEntity.ClassName,
                SubjectId = classEntity.SubjectId,
                SubjectCode = classEntity.Subject.SubjectCode,
                SubjectName = classEntity.Subject.SubjectName,
                LecturerId = classEntity.LecturerId!.Value,
                LecturerCode = classEntity.Lecturer.LecturerCode,
                LecturerName = classEntity.Lecturer.Fullname,
                EnrolKey = classEntity.EnrolKey,
                MemberCount = classEntity.MemberCount,
                TeamCount = classEntity.TeamCount,
                CreatedDate = classEntity.CreatedDate,
                IsActive = classEntity.IsActive,
                ClassFiles = classEntity.ClassFiles.Select(x => (ClassFileVM)x).ToList(),
                ClassMembers = classEntity.ClassMembers.Select(x => (ClassMemberVM)x).ToList(),
                ProjectAssignments = classEntity.ProjectAssignments.Select(x => (ProjectAssignmentVM)x).ToList(),
                Teams = classEntity.Teams.Select(x => (TeamVM)x).ToList(),
            };
        }
    }
}
