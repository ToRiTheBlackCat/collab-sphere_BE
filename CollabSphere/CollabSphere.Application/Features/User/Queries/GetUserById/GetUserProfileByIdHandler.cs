using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Classes.Queries.GetClassById;
using CollabSphere.Application.Mappings.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.User.Queries.GetUserById
{
    public class GetUserProfileByIdHandler : QueryHandler<GetUserProfileByIdQuery, GetUserProfileByIdResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserProfileByIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetUserProfileByIdResult> HandleCommand(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
        {
            var result = new GetUserProfileByIdResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
                Authorized = true,
            };

            try
            {
                //Find existed User
                var foundUser = _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.UserId).Result;

                if (foundUser != null && (foundUser.Lecturer != null || foundUser.Student != null))
                {
                    //Check if lecturer or student
                    if (foundUser.IsTeacher)
                    {
                        var lecturerProfile = foundUser.Lecturer_To_UserProfileDto();
                        result.User = lecturerProfile;
                    }
                    else
                    {
                        var studentProfile = foundUser.Student_To_UserProfileDto();
                        result.User = studentProfile;
                    }
                    result.IsSuccess = true;
                    result.Message = $"Get profile of User with that id: {request.UserId} successfully";
                }
                else
                {
                    result.Message = $"Cannot find any User with that id: {request.UserId}";
                    result.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetUserProfileByIdQuery request)
        {
            //Check existed user
            var foundUser = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.UserId);
            if (foundUser == null)
            {
                errors.Add(new OperationError()
                {
                    Field = " UserId",
                    Message = $"Cannot find any User with that id: {request.UserId}"
                });
                return;
            }
        }
    }
}
