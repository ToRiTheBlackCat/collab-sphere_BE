using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Cache
{
    public class TempSignUpOTPCache
    {
        public string OtpCode { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
    }
}
