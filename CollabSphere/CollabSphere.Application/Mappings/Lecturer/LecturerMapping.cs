using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Mappings.Lecturer
{
    public static class LecturerMapping
    {
        public static List<LecturerResponseDto> ListUser_To_LecturerResponseDto(this List<Domain.Entities.User>? userList)
        {
            var dtoList = new List<LecturerResponseDto>();
            if (userList == null)
            {
                return dtoList;
            }

            foreach (var user in userList)
            {
                var mappedDto = new LecturerResponseDto
                {
                    UId = user.UId,
                    Email = user.Email,
                    IsTeacher = user.IsTeacher,
                    RoleId = user.RoleId,
                    RoleName = user.Role.RoleName,
                    Fullname = user.Lecturer.Fullname,
                    Address = user.Lecturer.Address,
                    PhoneNumber = user.Lecturer.PhoneNumber,
                    Yob = user.Lecturer.Yob,
                    AvatarPublicId = user.Lecturer.AvatarImg,
                    School = user.Lecturer.School,
                    LecturerCode = user.Lecturer.LecturerCode,
                    Major = user.Lecturer.Major,
                    CreatedDate = user.CreatedDate,
                    IsActive = user.IsActive,
                };

                dtoList.Add(mappedDto);
            }
            return dtoList;
        }
    }
}
