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
            var errorList = new List<OperationError>();
            await ValidateRequest(errorList, request);

            if (errorList.Any())
            {
                return new TResult
                {
                    IsSuccess = false,
                    IsValidInput = false,
                    ErrorList = errorList
                };
            }

            return await HandleCommand(errorList, request, cancellationToken);
        }

        protected abstract Task<TResult> HandleCommand(List<OperationError> errors, TRequest request, CancellationToken cancellationToken);
        protected abstract Task ValidateRequest(List<OperationError> errors, TRequest request);
    }

    public abstract class BaseCommandHandler<TRequest> : BaseCommandHandler<TRequest, BaseCommandResult>
        where TRequest : ICommand<BaseCommandResult>
    {
    }
}
