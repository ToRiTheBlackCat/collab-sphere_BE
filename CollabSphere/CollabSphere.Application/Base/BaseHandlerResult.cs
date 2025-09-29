using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Base
{
    public class BaseHandlerResult
    {
        public List<OperationError> ErrorList { get; set; } = new();
        public bool IsValidInput { get; set; } = true;
        public bool IsSuccess { get; set; } = false;
        public string Message { get; set; } = string.Empty;
    }
}
