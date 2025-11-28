using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWhiteboard.Commands.UpdatePageTitle
{
    public class UpdatePageTitleHandler : CommandHandler<UpdatePageTitleCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePageTitleHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(UpdatePageTitleCommand request, CancellationToken cancellationToken)
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
                    foundPage.PageTitle = request.NewPageTitle.Trim();

                    _unitOfWork.WhiteboardPageRepo.Update(foundPage);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    var jsonOptions = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles,
                        WriteIndented = true
                    };
                    result.IsSuccess = true;
                    result.Message = JsonSerializer.Serialize(foundPage, jsonOptions);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UpdatePageTitleCommand request)
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
    }
}
