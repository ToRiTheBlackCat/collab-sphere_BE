using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.SubjectModels;

namespace CollabSphere.Application.Features.Subjects.Queries.GetSubjectById
{
    public class GetSubjectByIdResult : QueryResult
    {
        public SubjectVM? Subject { get; set; }
    }
}
