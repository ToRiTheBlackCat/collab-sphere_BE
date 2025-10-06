using CollabSphere.Application.DTOs.Student;
using CollabSphere.Application.Mappings.Student;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Student.Commands
{
    public class GetAllStudentHandler : IRequestHandler<GetAllStudentCommand, List<GetAllStudentResponseDto>?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configure;
        private readonly ILogger<GetAllStudentHandler> _logger;

        public GetAllStudentHandler(IUnitOfWork unitOfWork,
                                    IConfiguration configure,
                                    ILogger<GetAllStudentHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _configure = configure;
            _logger = logger;
        }

        public async Task<List<GetAllStudentResponseDto>?> Handle(GetAllStudentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var studentList = await _unitOfWork.StudentRepo
                    .SearchStudent(
                         request.Dto.Email,
                         request.Dto.FullName,
                         request.Dto.Yob,
                         request.Dto.StudentCode,
                         request.Dto.Major,
                         request.Dto.PageNumber,
                         request.Dto.PageSize,
                         request.Dto.IsDesc
                    );
                var mappedList = studentList.ListUser_To_ListGetAllStudentResponseDto();
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
