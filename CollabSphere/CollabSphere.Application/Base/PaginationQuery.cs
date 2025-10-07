using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Base
{
    public abstract class PaginationQuery
    {
        [FromQuery]
        public virtual bool ViewAll { get; set; } = false;

        [FromQuery]
        public virtual int PageNum { get; set; } = 1;

        [FromQuery]
        public virtual int PageSize { get; set; } = 10;
    }
}
