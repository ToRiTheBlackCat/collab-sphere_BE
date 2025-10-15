using CollabSphere.Application.DTOs.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Mappings.Student
{
    public static class LecturerMapping
    {
        public static List<StudentResponseDto> ListUser_To_StudentResponseDto(this List<Domain.Entities.User>? userList)
        {
            var dtoList = new List<StudentResponseDto>();
            if (userList == null)
            {
                return dtoList;
            }

            foreach (var user in userList)
            {
                var mappedDto = new StudentResponseDto
                {
                    UId = user.UId,
                    Email = user.Email,
                    IsTeacher = user.IsTeacher,
                    RoleId = user.RoleId,
                    RoleName = user.Role.RoleName,
                    Fullname = user.Student.Fullname,
                    Address = user.Student.StudentCode,
                    PhoneNumber = user.Student.StudentCode,
                    Yob = user.Student.Yob,
                    AvatarPublicId = user.Student.AvatarImg,
                    School = user.Student.School,
                    StudentCode = user.Student.StudentCode,
                    Major = user.Student.Major,
                    CreatedDate = user.CreatedDate,
                    IsActive = user.IsActive,
                };

                dtoList.Add(mappedDto);
            }
            return dtoList;
        }
    }
}
