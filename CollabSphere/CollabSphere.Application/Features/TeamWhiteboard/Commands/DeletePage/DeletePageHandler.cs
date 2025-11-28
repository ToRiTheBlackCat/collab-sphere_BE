using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWhiteboard.Commands.DeletePage
{
    public class DeletePageHandler : CommandHandler<DeletePageCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeletePageHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(DeletePageCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var foundPage = await _unitOfWork.WhiteboardPageRepo.GetById(request.PageId);
                if (foundPage != null)
                {
                    //Bind to dto
                    var dto = new DeletePageResponseDTO
                    {
                        WhiteboardId = foundPage.WhiteboardId,
                        PageId = foundPage.PageId,
                    };

                    _unitOfWork.WhiteboardPageRepo.Delete(foundPage);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    var jsonOptions = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles,
                        WriteIndented = true
                    };
                    result.IsSuccess = true;
                    result.Message = JsonSerializer.Serialize(dto, jsonOptions);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, DeletePageCommand request)
        {
            var foundPage = await _unitOfWork.WhiteboardPageRepo.GetById(request.PageId);
            if (foundPage == null)
            {
                errors.Add(new OperationError()
                {
                    Field = "PageId",
                    Message = $"Not found any Page with ID: {request.PageId}"
                });
                return;
            }
        }
        public class DeletePageResponseDTO
        {
            public int? WhiteboardId { get; set; }
            public int? PageId { get; set; }
        }
    }
}
