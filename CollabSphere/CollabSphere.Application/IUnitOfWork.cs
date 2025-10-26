﻿
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
        IClassMemberRepository ClassMemberRepo { get; }
        IClassRepository ClassRepo { get; }
        IDocumentStateRepository DocStateRepo { get; }
        ILecturerRepository LecturerRepo { get; }
        IObjectiveRepository ObjectiveRepo { get; }
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
        IEvaluationDetailRepository EvaluationDetailRepo { get; }

        //More IRepo below

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> SaveChangesAsync();
    }
}
