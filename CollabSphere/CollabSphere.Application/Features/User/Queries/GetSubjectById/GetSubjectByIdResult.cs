using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.SubjectModels;

namespace CollabSphere.Application.Features.User.Queries.GetSubjectById
{
    public class GetSubjectByIdResult : BaseQueryResult
    {
        public SubjectVM? Subject { get; set; }
    }
}
