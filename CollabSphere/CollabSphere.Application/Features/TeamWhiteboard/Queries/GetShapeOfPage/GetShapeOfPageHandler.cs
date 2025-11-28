using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.TeamWhiteboard.Queries.GetWhiteboardPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWhiteboard.Queries.GetShapeOfPage
{
    public class GetShapeOfPageHandler : QueryHandler<GetShapeOfPageQuery, GetShapeOfPageResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetShapeOfPageHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected override async Task<GetShapeOfPageResult> HandleCommand(GetShapeOfPageQuery request, CancellationToken cancellationToken)
        {
            var result = new GetShapeOfPageResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var foundPage = await _unitOfWork.WhiteboardPageRepo.GetById(request.PageId);
                if (foundPage != null)
                {
                    var pageShapes = await _unitOfWork.ShapeRepo.GetShapesOfPage(foundPage.PageId);
                    result.Shapes = pageShapes;
                }
                await _unitOfWork.CommitTransactionAsync();
                result.IsSuccess = true;
                result.Message = $"Get shapes of page with ID: {request.PageId} successfully";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetShapeOfPageQuery request)
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
