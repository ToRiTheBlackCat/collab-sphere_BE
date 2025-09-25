using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.User.Queries.GetSubjectById
{
    public class GetSubjectByIdResult : BaseQueryResult
    {
        public SubjectVM? Subject { get; set; }
    }
}
