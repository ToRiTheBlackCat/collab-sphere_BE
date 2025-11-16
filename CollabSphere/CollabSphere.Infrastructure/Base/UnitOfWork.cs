using CollabSphere.Application;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Domain.Interfaces;
using CollabSphere.Infrastructure.PostgreDbContext;
using CollabSphere.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Infrastructure.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly collab_sphereContext _context;
        private IDbContextTransaction? _transaction;

        #region Register_Repo
        public IUserRepository UserRepo { get; }
        public ICheckpointAssignmentRepository CheckpointAssignmentRepo { get; }
        public ICheckpointFileRepository CheckpointFileRepo { get; }
        public ICheckpointRepository CheckpointRepo { get; }
        public IClassFileRepository ClassFileRepo { get; }
        public IClassMemberRepository ClassMemberRepo { get; }
        public IClassRepository ClassRepo { get; }
        public ILecturerRepository LecturerRepo { get; }
        public IObjectiveRepository ObjectiveRepo { get; }
        public IObjectiveMilestoneRepository ObjectiveMilestoneRepo { get; }
        public IProjectAssignmentRepository ProjectAssignmentRepo { get; }
        public IProjectRepository ProjectRepo { get; }
        public ISemesterRepository SemesterRepo { get; }
        public IStudentRepository StudentRepo { get; }
        public ISubjectGradeComponentRepository SubjectGradeComponentRepo { get; }
        public ISubjectOutcomeRepository SubjectOutcomeRepo { get; }
        public ISubjectRepository SubjectRepo { get; }
        public ISubjectSyllabusRepository SubjectSyllabusRepo { get; }
        public ITeamMilestoneRepository TeamMilestoneRepo { get; }
        public ITeamRepository TeamRepo { get; }
        public ITeamEvaluationRepository TeamEvaluationRepo { get; }
        public ITeamFileRepository TeamFileRepo { get; }
        public IEvaluationDetailRepository EvaluationDetailRepo { get; }
        public IMemberEvaluationRepository MemberEvaluationRepo { get; }
        public IMilestoneFileRepository MilestoneFileRepo { get; }
        public IMilestoneQuestionRepository MilestoneQuestionRepo { get; }
        public IMilestoneQuestionAnsRepository MilestoneQuestionAnsRepo { get; }
        public IAnswerEvaluationRepository AnswerEvaluationRepo { get; }
        public IMilestoneEvaluationRepository MilestoneEvaluationRepo { get; }
        public IMilestoneReturnRepository MilestoneReturnRepo { get; }
        public IProjectRepoMappingRepository ProjectRepoMappingRepo { get; }
        public IMeetingRepository MeetingRepo { get; }
        public ITeamWorkspaceRepository TeamWorkspaceRepo { get; }
        public IListRepository ListRepo { get; }
        public ICardRepository CardRepo { get; }
        public ICardAssignmentRepository CardAssignmentRepo { get; }
        public ITaskRepository TaskRepo { get; }
        public ISubTaskRepository SubTaskRepo { get; }

        public IPrAnalysisRepository PrAnalysisRepo { get; }
        #endregion

        public UnitOfWork(collab_sphereContext context)
        {
            _context = context;

            #region Register_Repo
            UserRepo = new UserRepository(_context);
            CheckpointAssignmentRepo = new CheckpointAssignmentRepository(_context);
            CheckpointFileRepo = new CheckpointFileRepository(_context);
            CheckpointRepo = new CheckpointRepository(_context);
            ClassFileRepo = new ClassFileRepository(_context);
            ClassMemberRepo = new ClassMemberRepositiory(_context);
            ClassRepo = new ClassRepository(_context);
            LecturerRepo = new LecturerRepository(_context);
            ObjectiveRepo = new ObjectiveRepository(_context);
            ObjectiveMilestoneRepo = new ObjectiveMilestoneRepository(_context);
            ProjectAssignmentRepo = new ProjectAssignmentRepository(_context);
            ProjectRepo = new ProjectRepository(_context);
            SemesterRepo = new SemesterRepository(_context);
            StudentRepo = new StudentRepository(_context);
            SubjectGradeComponentRepo = new SubjectGradeComponentRepository(_context);
            SubjectOutcomeRepo = new SubjectOutcomeRepository(_context);
            SubjectRepo = new SubjectRepository(_context);
            SubjectSyllabusRepo = new SubjectSyllabusRepository(_context);
            TeamMilestoneRepo = new TeamMilestoneRepository(_context);
            TeamRepo = new TeamRepository(_context);
            TeamMilestoneRepo = new TeamMilestoneRepository(_context);
            TeamEvaluationRepo = new TeamEvaluationRepository(_context);
            TeamFileRepo = new TeamFileRepository(_context);
            EvaluationDetailRepo = new EvaluationDetailRepository(_context);
            MemberEvaluationRepo = new MemberEvaluationRepository(_context);
            MilestoneFileRepo = new MilestoneFileRepository(_context);
            MilestoneQuestionRepo = new MilestoneQuestionRepository(_context);
            MilestoneQuestionAnsRepo = new MilestoneQuestionAnsRepository(_context);
            AnswerEvaluationRepo = new AnswerEvaluationRepository(_context);
            MilestoneEvaluationRepo = new MilestoneEvaluationRepository(_context);
            MilestoneReturnRepo = new MilestoneReturnRepository(_context);
            ProjectRepoMappingRepo = new ProjectRepoMappingRepository(_context);
            MeetingRepo = new MeetingRepository(_context);
            TeamWorkspaceRepo = new TeamWorkspaceRepository(_context);
            ListRepo = new ListRepository(_context);
            CardRepo = new CardRepository(_context);
            CardAssignmentRepo = new CardAssignmentRepository(_context);
            TaskRepo = new TaskRepository(_context);
            SubTaskRepo = new SubTaskRepository(_context);

            PrAnalysisRepo = new PrAnalysisRepository(_context);
            #endregion
        }
        
        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
