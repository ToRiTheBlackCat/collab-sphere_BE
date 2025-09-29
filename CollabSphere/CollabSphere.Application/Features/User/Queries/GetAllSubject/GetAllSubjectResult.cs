using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.SubjectModels;

namespace CollabSphere.Application.Features.User.Queries.GetAllSubject
{
    public class GetAllSubjectResult : QueryResult
    {
        public List<SubjectVM> Subjects { get; set; } = new();
    }
}
