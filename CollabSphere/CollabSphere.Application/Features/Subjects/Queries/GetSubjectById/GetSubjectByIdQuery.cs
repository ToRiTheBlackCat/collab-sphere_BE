﻿using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Subjects.Queries.GetSubjectById
{
    public class GetSubjectByIdQuery : IQuery<GetSubjectByIdResult>
    {
        public int? SubjectId { get; set; } = null;
    }
}
