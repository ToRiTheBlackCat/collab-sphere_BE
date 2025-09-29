using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Base
{
    public interface ICommand<TResult> : IRequest<TResult>
    {
    }

    public interface ICommand : ICommand<CommandResult>
    {
    }
}
