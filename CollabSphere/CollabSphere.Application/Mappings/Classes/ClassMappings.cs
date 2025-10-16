using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Mappings.Classes
{
    public static class ClassMappings
    {
        public static ClassVM ToClassVM(this Class classEntity, bool showEnrolKey = false)
        {
            return new ClassVM()
            {
                ClassId = classEntity.ClassId,
                ClassName = classEntity.ClassName,
                SubjectId = classEntity.Subject.SubjectId,
                SubjectCode = classEntity.Subject.SubjectCode,
                SubjectName = classEntity.Subject.SubjectName,
                LecturerId = classEntity.Lecturer.LecturerId,
                LecturerCode = classEntity.Lecturer.LecturerCode,
                LecturerName = classEntity.Lecturer.Fullname,
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
