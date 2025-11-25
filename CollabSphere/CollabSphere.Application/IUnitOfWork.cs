
using CollabSphere.Domain.Intefaces;
using CollabSphere.Domain.Interfaces;
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
        IChatConversationRepository ChatConversationRepo { get; }
        IChatMessageRepository ChatMessageRepo { get; }
        ICheckpointAssignmentRepository CheckpointAssignmentRepo { get; }
        ICheckpointFileRepository CheckpointFileRepo { get; }
        ICheckpointRepository CheckpointRepo { get; }
        IClassFileRepository ClassFileRepo { get; }
        IClassMemberRepository ClassMemberRepo { get; }
        IClassRepository ClassRepo { get; }
        IDocumentRoomRepository DocRoomRepo { get; }
        IDocumentStateRepository DocStateRepo { get; }
        IGithubConnectionStateRepository GithubConnectionStateRepo { get; }
        ILecturerRepository LecturerRepo { get; }
        IObjectiveRepository ObjectiveRepo { get; }
        INotificationRecipientRepository NotificationRecipientRepo { get; }
        INotificationRepository NotificationRepo { get; }
        IObjectiveMilestoneRepository ObjectiveMilestoneRepo { get; }
        IProjectAssignmentRepository ProjectAssignmentRepo { get; }
        IProjectRepository ProjectRepo { get; }
        ISemesterRepository SemesterRepo { get; }
        IStudentRepository StudentRepo { get; }
        ISubjectGradeComponentRepository SubjectGradeComponentRepo { get; }
        ISubjectOutcomeRepository SubjectOutcomeRepo { get; }
        ISubjectRepository SubjectRepo { get; }
        ISubjectSyllabusRepository SubjectSyllabusRepo { get; }
        ITeamMilestoneRepository TeamMilestoneRepo { get; }
        ITeamRepository TeamRepo { get; }
        ITeamEvaluationRepository TeamEvaluationRepo { get; }
        ITeamFileRepository TeamFileRepo { get; }
        IEvaluationDetailRepository EvaluationDetailRepo { get; }
        IMemberEvaluationRepository MemberEvaluationRepo { get; }
        IMessageRecipientRepository MessageRecipientRepo { get; }
        IMilestoneFileRepository MilestoneFileRepo { get; }
        IMilestoneQuestionRepository MilestoneQuestionRepo { get; }
        IMilestoneQuestionAnsRepository MilestoneQuestionAnsRepo { get; }
        IAnswerEvaluationRepository AnswerEvaluationRepo { get; }
        IMilestoneEvaluationRepository MilestoneEvaluationRepo { get; }
        IMilestoneReturnRepository MilestoneReturnRepo { get; }
        IProjectRepoMappingRepository ProjectRepoMappingRepo { get; }
        IMeetingRepository MeetingRepo { get; }
        ITeamWorkspaceRepository TeamWorkspaceRepo { get; }
        IListRepository ListRepo { get; }
        ICardRepository CardRepo { get; }
        ITaskRepository TaskRepo { get; }
        ISubTaskRepository SubTaskRepo { get; }
        ICardAssignmentRepository CardAssignmentRepo { get; }
        IPrAnalysisRepository PrAnalysisRepo { get; }
        //More IRepo below

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> SaveChangesAsync();
    }
}
