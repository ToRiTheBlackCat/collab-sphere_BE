﻿using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Queries.GetStudentClasses
{
    public class GetStudentClassesResult : QueryResult
    {
        public PagedList<ClassVM>? PaginatedClasses { get; set; }
    }
}
