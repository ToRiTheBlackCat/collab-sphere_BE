using CollabSphere.Application.DTOs.Validation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CollabSphere.Application.Base
{
    public abstract class BaseCommandHandler<TRequest, TResult> : IRequestHandler<TRequest, TResult>
        where TRequest : ICommand<TResult>
        where TResult : BaseCommandResult, new()
    {
        public async Task<TResult> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var result = new TResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };
            var errorList = new List<OperationError>();
            try
            {
                await ValidateRequest(errorList, request);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            if (errorList.Any())
            {
                result.ErrorList = errorList;
                result.IsValidInput = false;
                return result;
            }

            result = await HandleCommand(request, cancellationToken);

            return result;
        }

        protected abstract Task<TResult> HandleCommand(TRequest request, CancellationToken cancellationToken);
        protected abstract Task ValidateRequest(List<OperationError> errors, TRequest request);
    }

    public abstract class BaseCommandHandler<TRequest> : BaseCommandHandler<TRequest, BaseCommandResult>
        where TRequest : ICommand<BaseCommandResult>
    {
    }
}
