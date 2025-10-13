using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.Features.Student.Commands;
using CollabSphere.Application.Mappings.Lecturer;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Lecturer.Commands
{
    public class GetAllLecturerHandler : IRequestHandler<GetAllLecturerCommand, GetAllLecturerResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAllLecturerHandler> _logger;

        public GetAllLecturerHandler(IUnitOfWork unitOfWork,
                                    ILogger<GetAllLecturerHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<GetAllLecturerResponseDto> Handle(GetAllLecturerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = new GetAllLecturerResponseDto
                {
                    ItemCount = 0,
                    PageNum = request.Dto.PageNumber,
                    PageSize = request.Dto.PageSize
                };

                await _unitOfWork.BeginTransactionAsync();

                var lecturerList = await _unitOfWork.LecturerRepo
                    .SearchLecturer(
                         request.Dto.Email,
                         request.Dto.FullName,
                         request.Dto.Yob,
                         request.Dto.LecturerCode,
                         request.Dto.Major,
                         request.Dto.PageNumber,
                         request.Dto.PageSize,
                         request.Dto.IsDesc
                    );
                var mappedList = lecturerList.ListUser_To_LecturerResponseDto();
                await _unitOfWork.CommitTransactionAsync();

                result.ItemCount = mappedList.Count;
                result.LecturerList = mappedList;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Have exception when get all lecturer account");
                _logger.LogInformation("Detail of exception: " + ex.Message.ToString());
                await _unitOfWork.RollbackTransactionAsync();
                return null;
            }
        }
    }
}
