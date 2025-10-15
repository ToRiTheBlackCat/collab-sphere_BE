using CollabSphere.Application.DTOs.Validation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Base
{
    public abstract class BaseHandler<TRequest, TResult> : IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>
        where TResult : BaseHandlerResult, new()
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
                return result;
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

    public abstract class CommandHandler<TRequest> : BaseHandler<TRequest, CommandResult>
        where TRequest : ICommand
    {
    }

    public abstract class CommandHandler<Request, Result> : BaseHandler<Request, Result>
        where Request : ICommand<Result>
        where Result : CommandResult, new()
    {
    }

    public abstract class QueryHandler<TQuery, TResult> : BaseHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
        where TResult : QueryResult, new()
    {

    }
}
