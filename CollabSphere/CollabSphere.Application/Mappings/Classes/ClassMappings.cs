using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Mappings.Classes
{
    public static partial class ClassMappings
    {
        public static ClassVM ToClassVM(this Class classEntity, bool showEnrolKey = false)
        {
            return new ClassVM()
            {
                ClassId = classEntity.ClassId,
                ClassName = classEntity.ClassName,
                SubjectId = classEntity.SubjectId,
                SubjectCode = classEntity.Subject != null ? classEntity.Subject.SubjectCode : "NOT FOUND",
                SubjectName = classEntity.Subject != null ? classEntity.Subject.SubjectName : "NOT FOUND",
                SemesterId = classEntity.SemesterId,
                SemesterName = classEntity.Semester != null ? classEntity.Semester.SemesterName : "NOT FOUND",
                LecturerId = classEntity.LecturerId,
                LecturerCode = classEntity.Lecturer != null ? classEntity.Lecturer.LecturerCode : "NOT FOUND",
                LecturerName = classEntity.Lecturer != null ? classEntity.Lecturer.Fullname : "NOT FOUND",
                EnrolKey = showEnrolKey ? classEntity.EnrolKey : "",
                MemberCount = classEntity.MemberCount,
                TeamCount = classEntity.TeamCount,
                CreatedDate = classEntity.CreatedDate,
                IsActive = classEntity.IsActive,
            };
        }

        public static List<ClassVM> ToClassVM(this IEnumerable<Class>? classEntityList, bool showEnrolKey = false)
        {
            if (classEntityList == null || !classEntityList.Any())
            {
                return new List<ClassVM>();
            }

            return classEntityList.Select(cls => cls.ToClassVM(showEnrolKey)).ToList();
        }
    }
}
