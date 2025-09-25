using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.User.Queries.GetAllSubject
{
    public class GetAllSubjectResult : BaseQueryResult
    {
        public List<SubjectVM> Subjects { get; set; } = new();
    }
}
