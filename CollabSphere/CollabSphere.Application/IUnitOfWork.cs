
using CollabSphere.Domain.Intefaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application
{
    public interface IUnitOfWork : IDisposable
    {

        IUserRepository UserRepo { get; }
        IClassMemberRepository ClassMemberRepo { get; }
        IClassRepository ClassRepo { get; }
        ILecturerRepository LecturerRepo { get; }
        IObjectiveRepository ObjectiveRepo { get; }
        IObjectiveMilestoneRepository ObjectiveMilestoneRepo { get; }
        IProjectAssignmentRepository ProjectAssignmentRepo { get; }
        IProjectRepository ProjectRepo { get; }
        IStudentRepository StudentRepo { get; }
        ISubjectGradeComponentRepository SubjectGradeComponentRepo { get; }
        ISubjectOutcomeRepository SubjectOutcomeRepo { get; }
        ISubjectRepository SubjectRepo { get; }
        ISubjectSyllabusRepository SubjectSyllabusRepo { get; }
        ITeamRepository TeamRepo { get; }

        //More IRepo below

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> SaveChangesAsync();
    }
}
