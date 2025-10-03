using CollabSphere.Application.DTOs.User;
using CollabSphere.Application.Features.User.Queries;
using CollabSphere.Application.Mappings.User;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Admin.Queries
{
    public record AdminGetAllUsersQuery() : IRequest<AdminGetAllUsersResponseDto>;
    public class AdminGetAllUsersHandler : IRequestHandler<AdminGetAllUsersQuery, AdminGetAllUsersResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminGetAllUsersHandler> _logger;
        public AdminGetAllUsersHandler(IUnitOfWork unitOfWork,
                                       ILogger<AdminGetAllUsersHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AdminGetAllUsersResponseDto> Handle(AdminGetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var response = new AdminGetAllUsersResponseDto();
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //Get list of department
                var headDepartList = await _unitOfWork.UserRepo.GetAllHeadDepartAsync();

                //Get list of staff
                var staffList = await _unitOfWork.UserRepo.GetAllStaffAsync();

                //Get list of lecturer
                var lecturerList = await _unitOfWork.UserRepo.GetAllLecturerAsync();

                //Get list of student
                var studentList = await _unitOfWork.UserRepo.GetAllStudentAsync();

                //Set list count to response
                response.HeadDepartmentCount = headDepartList != null ? headDepartList.Count() : 0;
                response.StaffCount = staffList != null ? staffList.Count() : 0;
                response.LecturerCount = lecturerList != null ? lecturerList.Count() : 0;
                response.StudentCount = studentList != null ? studentList.Count() : 0;
                //Set data list to response
                response.HeadDepartmentList = headDepartList.ListUser_To_Admin_ListAllHeadDepartment_StaffDto();
                response.StaffList = staffList.ListUser_To_Admin_ListAllHeadDepartment_StaffDto();
                response.LecturerList = lecturerList.ListUser_To_ListAdmin_AllLecturerDto();
                response.StudentList = studentList.ListUser_To_ListAdmin_AllStudentDto();

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Have exception when getting list users");
                _logger.LogInformation("Detail of exception: " + ex.Message.ToString());

                return response;

            }
        }
    }
}
