using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Classes
{
    public class ClassVM
    {
        public int ClassId { get; set; }

        public string ClassName { get; set; } = string.Empty;

        public int SubjectId { get; set; }

        public string SubjectCode { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public int SemesterId { get; set; } = -1;

        public string SemesterName { get; set; } = string.Empty;

        public int? LecturerId { get; set; }

        public string LecturerCode { get; set; } = string.Empty;

        public string LecturerName { get; set; } = string.Empty;

        public string EnrolKey { get; set; }

        public int? MemberCount { get; set; }

        public int? TeamCount { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }

        //public static explicit operator ClassVM(Class classEntity)
        //{
        //    return new ClassVM()
        //    {
        //        ClassId = classEntity.ClassId,
        //        ClassName = classEntity.ClassName,
        //        SubjectId = classEntity.Subject.SubjectId,
        //        SubjectCode = classEntity.Subject.SubjectCode,
        //        SubjectName = classEntity.Subject.SubjectName,
        //        LecturerId = classEntity.Lecturer.LecturerId,
        //        LecturerCode = classEntity.Lecturer.LecturerCode,
        //        LecturerName = classEntity.Lecturer.Fullname,
        //        EnrolKey = classEntity.EnrolKey,
        //        MemberCount = classEntity.MemberCount,
        //        TeamCount = classEntity.TeamCount,
        //        CreatedDate = classEntity.CreatedDate,
        //        IsActive = classEntity.IsActive,
        //    };
        //}
    }
}
