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
    public class GetAllLecturerHandler : IRequestHandler<GetAllLecturerCommand, List<GetAllLecturerResponseDto>?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configure;
        private readonly ILogger<GetAllLecturerHandler> _logger;

        public GetAllLecturerHandler(IUnitOfWork unitOfWork,
                                    IConfiguration configure,
                                    ILogger<GetAllLecturerHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _configure = configure;
            _logger = logger;
        }

        public async Task<List<GetAllLecturerResponseDto>?> Handle(GetAllLecturerCommand request, CancellationToken cancellationToken)
        {
            try
            {
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
                var mappedList = lecturerList.ListUser_To_ListGetAllLecturerResponseDto();
                await _unitOfWork.CommitTransactionAsync();

                return mappedList;
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
