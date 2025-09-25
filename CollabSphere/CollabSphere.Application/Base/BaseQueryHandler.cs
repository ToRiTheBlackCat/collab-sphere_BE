using CollabSphere.Application.DTOs.Validation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Base
{
    public abstract class BaseQueryHandler<TQuery, TResult> : IRequestHandler<TQuery, TResult>
        where TQuery : IRequest<TResult>
        where TResult : BaseQueryResult, new()
    {
        public async Task<TResult> Handle(TQuery request, CancellationToken cancellationToken)
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

            return await HandleCommand(request, cancellationToken);
        }

        protected abstract Task<TResult> HandleCommand(TQuery request, CancellationToken cancellationToken);
        protected abstract Task ValidateRequest(List<OperationError> errors, TQuery request);
    }
}
