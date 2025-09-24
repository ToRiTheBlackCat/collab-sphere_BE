using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.OTP
{
    public class SendOTPCommand : IRequest<(bool, string)>
    {
        public string Email { get; set; }
        public SendOTPCommand(string email)
        {
            Email = email;
        }
    }
}
