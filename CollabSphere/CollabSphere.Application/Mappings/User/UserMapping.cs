using CollabSphere.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Mappings.User
{
    public static class UserMapping
    {
        public static List<Admin_AllHeadDepartment_StaffDto> ListUser_To_Admin_ListAllHeadDepartment_StaffDto(this List<Domain.Entities.User>? userList)
        {
            var dtoList = new List<Admin_AllHeadDepartment_StaffDto>();
            if(userList == null)
            {
                return dtoList;
            }

            foreach (var user in userList)
            {
                var mappedDto = new Admin_AllHeadDepartment_StaffDto
                {
                    Email = user.Email,
                    RoleName = user.Role.RoleName,
                    IsTeacher = user.IsTeacher,
                    CreatedDate = user.CreatedDate,
                    IsActive = user.IsActive,
                };

                dtoList.Add(mappedDto);
            }
            return dtoList;
        }

        public static List<Admin_AllLecturerDto> ListUser_To_ListAdmin_AllLecturerDto(this List<Domain.Entities.User>? userList)
        {
            var dtoList = new List<Admin_AllLecturerDto>();
            if (userList == null)
            {
                return dtoList;
            }

            foreach (var user in userList)
            {
                var mappedDto = new Admin_AllLecturerDto
                {
                    Email = user.Email,
                    RoleName = user.Role.RoleName,
                    IsTeacher = user.IsTeacher,
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

        public static List<Admin_AllStudentDto> ListUser_To_ListAdmin_AllStudentDto(this List<Domain.Entities.User>? userList)
        {
            var dtoList = new List<Admin_AllStudentDto>();
            if (userList == null)
            {
                return dtoList;
            }

            foreach (var user in userList)
            {
                var mappedDto = new Admin_AllStudentDto
                {
                    Email = user.Email,
                    RoleName = user.Role.RoleName,
                    IsTeacher = user.IsTeacher,
                    Fullname = user.Student.Fullname,
                    Address = user.Student.Address,
                    PhoneNumber = user.Student.PhoneNumber,
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
