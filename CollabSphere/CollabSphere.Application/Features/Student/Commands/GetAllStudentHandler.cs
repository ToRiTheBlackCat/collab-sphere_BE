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
    public class GetAllStudentHandler : IRequestHandler<GetAllStudentCommand, GetAllLecturerResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAllStudentHandler> _logger;

        public GetAllStudentHandler(IUnitOfWork unitOfWork,
                                    IConfiguration configure,
                                    ILogger<GetAllStudentHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<GetAllLecturerResponseDto> Handle(GetAllStudentCommand request, CancellationToken cancellationToken)
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
                var mappedList = studentList.ListUser_To_StudentResponseDto();
                await _unitOfWork.CommitTransactionAsync();

                result.ItemCount = mappedList.Count;
                result.StudentList = mappedList;

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
