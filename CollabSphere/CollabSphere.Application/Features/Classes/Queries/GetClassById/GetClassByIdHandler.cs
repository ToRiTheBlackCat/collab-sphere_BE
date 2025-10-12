using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Queries.GetClassById
{
    public class GetClassByIdHandler : QueryHandler<GetClassByIdQuery, GetClassByIdResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetClassByIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetClassByIdResult> HandleCommand(GetClassByIdQuery request, CancellationToken cancellationToken)
        {
            var result = new GetClassByIdResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
                Authorized = true,
            };

            try
            {
                var classEntity = await _unitOfWork.ClassRepo.GetById(request.ClassId);
                if (classEntity != null)
                {
                    // Viewer is a Student but NOT in Class
                    if (request.ViewerRole == RoleConstants.STUDENT && !classEntity.ClassMembers.Any(x => x.StudentId == request.ViewerUId))
                    {
                        result.Authorized = false;
                        result.Message = "You are not a Student in this Class.";
                    }
                    else
                    {
                        result.Class = (ClassDetailDto)classEntity;
                    }
                }

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetClassByIdQuery request)
        {
            return;
        }
    }
}
