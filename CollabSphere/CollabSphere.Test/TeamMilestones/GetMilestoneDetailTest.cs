using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.TeamMilestones;
using CollabSphere.Application.DTOs.Teams;
using CollabSphere.Application.Features.TeamMilestones.Queries.GetMilestoneDetail;
using CollabSphere.Application.Features.TeamMilestones.Queries.GetMilestonesByTeam;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace CollabSphere.Test.TeamMilestones
{
    public class GetMilestoneDetailTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITeamRepository> _teamRepoMock;
        private readonly Mock<ITeamMilestoneRepository> _teamMilestoneRepoMock;

        private readonly GetMilestoneDetailHandler _handler;

        public GetMilestoneDetailTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _teamRepoMock = new Mock<ITeamRepository>();
            _teamMilestoneRepoMock = new Mock<ITeamMilestoneRepository>();

            _unitOfWorkMock.Setup(x => x.TeamRepo).Returns(_teamRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.TeamMilestoneRepo).Returns(_teamMilestoneRepoMock.Object);

            _handler = new GetMilestoneDetailHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnDetail_WhenValidQuery()
        {
            // Arrange
            var query = new GetMilestoneDetailQuery()
            {
                TeamMilestoneId = 10,
                UserId = 8,
                UserRole = RoleConstants.LECTURER,
            };

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                Title = "Team 1 Milestone 1",
                Description = "Description of Milestone 1",
                TeamId = 1,
                Team = new Domain.Entities.Team()
                {
                    Class = new Class()
                    {
                        ClassId = 1,
                        LecturerId = 8,
                    },
                    ClassMembers = new List<ClassMember>()
                    {

                    },
                },
                Checkpoints = new List<Checkpoint>()
                {

                },
                MilestoneQuestions = new List<MilestoneQuestion>()
                {

                },
                MilestoneEvaluation = null,
                MilestoneFiles = new List<MilestoneFile>()
                {

                },
                MilestoneReturns = new List<MilestoneReturn>()
                {

                },
                Progress = 87,
                StartDate = new DateOnly(),
                EndDate = new DateOnly(),
                Status = (int)TeamMilestoneStatuses.NOT_DONE,
            };
            _teamMilestoneRepoMock.Setup(x => x.GetDetailsById(10)).ReturnsAsync(milestone);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.TeamMilestone);
        }

        [Fact]
        public async Task Handle_ShouldReturnDetail_WhenMilestoneNotFound()
        {
            // Arrange
            var query = new GetMilestoneDetailQuery()
            {
                TeamMilestoneId = 10,
                UserId = 8,
                UserRole = RoleConstants.LECTURER,
            };

            _teamMilestoneRepoMock.Setup(x => x.GetById(10));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
            Assert.Null(result.TeamMilestone);
        }

        [Fact]
        public async Task Handle_ShouldReturnDetail_WhenNotStudentInTeam()
        {
            // Arrange
            var query = new GetMilestoneDetailQuery()
            {
                TeamMilestoneId = 10,
                UserId = 8,
                UserRole = RoleConstants.STUDENT,
            };

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                Title = "Team 1 Milestone 1",
                Description = "Description of Milestone 1",
                TeamId = 1,
                Team = new Domain.Entities.Team()
                {
                    Class = new Class()
                    {
                        ClassId = 1,
                        LecturerId = 8,
                    },
                    ClassMembers = new List<ClassMember>()
                    {
                        new ClassMember()
                        {
                            ClassMemberId = 1,
                            StudentId = 16,
                        },
                    },
                },
                Checkpoints = new List<Checkpoint>()
                {

                },
                MilestoneQuestions = new List<MilestoneQuestion>()
                {

                },
                MilestoneEvaluation = null,
                MilestoneFiles = new List<MilestoneFile>()
                {

                },
                MilestoneReturns = new List<MilestoneReturn>()
                {

                },
                Progress = 87,
                StartDate = new DateOnly(),
                EndDate = new DateOnly(),
                Status = (int)TeamMilestoneStatuses.NOT_DONE,
            };
            _teamMilestoneRepoMock.Setup(x => x.GetDetailsById(10)).ReturnsAsync(milestone);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("not a member of the team", result.ErrorList.First().Message);
        }


        [Fact]
        public async Task Handle_ShouldReturnDetail_WhenNotLecturerOfClass()
        {
            // Arrange
            var query = new GetMilestoneDetailQuery()
            {
                TeamMilestoneId = 10,
                UserId = 8,
                UserRole = RoleConstants.LECTURER,
            };

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                Title = "Team 1 Milestone 1",
                Description = "Description of Milestone 1",
                TeamId = 1,
                Team = new Domain.Entities.Team()
                {
                    Class = new Class()
                    {
                        ClassId = 1,
                        LecturerId = 16,
                    },
                    ClassMembers = new List<ClassMember>()
                    {
                    },
                },
                Checkpoints = new List<Checkpoint>()
                {

                },
                MilestoneQuestions = new List<MilestoneQuestion>()
                {

                },
                MilestoneEvaluation = null,
                MilestoneFiles = new List<MilestoneFile>()
                {

                },
                MilestoneReturns = new List<MilestoneReturn>()
                {

                },
                Progress = 87,
                StartDate = new DateOnly(),
                EndDate = new DateOnly(),
                Status = (int)TeamMilestoneStatuses.NOT_DONE,
            };
            _teamMilestoneRepoMock.Setup(x => x.GetDetailsById(10)).ReturnsAsync(milestone);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("You are not a lecturer of the class", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Casting_ShouldReturnDetail_WhenValidEntity()
        {
            // Arrange
            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                Title = "Team 1 Milestone 1",
                Description = "Description of Milestone 1",
                TeamId = 1,
                Checkpoints = new List<Checkpoint>()
                {
                    new Checkpoint()
                    {
                        CheckpointId = 1,
                        Title = "Checkpoint 1",
                        Description = "Check first description.",
                        StartDate = new DateOnly(2021, 2, 12),
                        DueDate = new DateOnly(2021, 2, 20),
                        CheckpointAssignments = new List<CheckpointAssignment>()
                        {
                            new CheckpointAssignment()
                            {
                                CheckpointAssignmentId = 1,
                                ClassMemberId = 2,
                                ClassMember = new ClassMember()
                                {
                                    ClassMemberId = 2,
                                    TeamRole = (int)TeamRole.LEADER,
                                    Student = new Student()
                                    {
                                        StudentId = 1,
                                        Fullname = "Stu1",
                                        StudentCode = "SE1234",
                                        AvatarImg = "avatar/student/avat_1.png",
                                        Yob = 2004,
                                    },
                                },
                            }
                        }
                    },
                },
                MilestoneQuestions = new List<MilestoneQuestion>()
                {
                    new MilestoneQuestion()
                    {
                        MilestoneQuestionId = 1,
                        TeamMilestoneId = 10,
                        TeamId = 1,
                        Question = "What is this question about?",
                        AnswerCount = 13,
                        CreatedTime = new DateTime(2025, 10, 25),
                    },
                },
                MilestoneEvaluation = new MilestoneEvaluation()
                {
                    MilestoneId = 10,
                    LecturerId = 1,
                    Comment = "Fully returned every task results.",
                    TeamId = 1,
                    Score = 87,
                    CreatedDate = new DateTime(2025, 12, 29),
                    Lecturer = new Lecturer()
                    {
                        LecturerId = 1,
                        LecturerCode = "LEC01",
                        Fullname = "Lecturer1",
                        PhoneNumber = "1234567890",
                        AvatarImg = "lecturer/avatar",
                    }
                },
                MilestoneFiles = new List<MilestoneFile>()
                {
                    new MilestoneFile()
                    {
                        FileId = 1,
                        TeamMilstoneId = 10,
                        FilePath = "files/reports/doc1-example.docx",
                        Type = "Word Doc"
                    },
                    new MilestoneFile()
                    {
                        FileId = 2,
                        TeamMilstoneId = 10,
                        FilePath = "files/reports/mvp-checklist.docx",
                        Type = "Word Doc"
                    },
                },
                MilestoneReturns = new List<MilestoneReturn>()
                {
                    new MilestoneReturn()
                    {
                        MileReturnId = 1,
                        ClassMemberId = 3,
                        FilePath = "files/return/mile1-context.xlse",
                        Type = "Excel Doc",
                        SubmitedDate = new DateTime(2022, 1, 12),
                        ClassMember = new ClassMember()
                        {
                            ClassMemberId= 3,
                            TeamRole = (int)TeamRole.MEMBER,
                            Student = new Student()
                            {
                                StudentId = 3,
                                Fullname = "Stu3",
                                StudentCode = "SE1333",
                                AvatarImg = "avatar/student/avat_3.png",
                                Yob = 2004,
                            },
                        }
                    },
                    new MilestoneReturn()
                    {
                        MileReturnId = 2,
                        ClassMemberId = 2,
                        FilePath = "files/return/mile1-doc-assign.xlse",
                        Type = "Word Doc",
                        SubmitedDate = new DateTime(2022, 2, 27),
                        ClassMember = new ClassMember()
                        {
                            ClassMemberId = 2,
                            TeamRole = (int)TeamRole.LEADER,
                            Student = new Student()
                            {
                                StudentId = 1,
                                Fullname = "Stu1",
                                StudentCode = "SE1234",
                                AvatarImg = "avatar/student/avat_1.png",
                                Yob = 2004,
                            },
                        },
                    }
                },
                Progress = 87,
                StartDate = new DateOnly(2020, 3, 12),
                EndDate = new DateOnly(2020, 6, 15),
                Status = (int)TeamMilestoneStatuses.DONE,
            };

            // Act
            var castedMilestoneDetail = (TeamMilestoneDetailDto)milestone;

            // Assert
            Assert.Equal(10, castedMilestoneDetail.TeamMilestoneId);
            Assert.Equal("Team 1 Milestone 1", castedMilestoneDetail.Title);
            Assert.Equal("Description of Milestone 1", castedMilestoneDetail.Description);
            Assert.Equal(1, castedMilestoneDetail.TeamId);
            Assert.Equal(87, castedMilestoneDetail.Progress);
            Assert.Equal(new DateOnly(2020, 3, 12), castedMilestoneDetail.StartDate);
            Assert.Equal(new DateOnly(2020, 6, 15), castedMilestoneDetail.EndDate);
            Assert.Equal((int)TeamMilestoneStatuses.DONE, castedMilestoneDetail.Status);

            Assert.Single(castedMilestoneDetail.Checkpoints);
            var checkpoint = castedMilestoneDetail.Checkpoints.First();
            Assert.Equal(1, checkpoint.CheckpointId);
            Assert.Equal("Checkpoint 1", checkpoint.Title);
            Assert.Equal("Check first description.", checkpoint.Description);
            Assert.Equal(new DateOnly(2021, 2, 12), checkpoint.StartDate);
            Assert.Equal(new DateOnly(2021, 2, 20), checkpoint.DueDate);

            Assert.Single(castedMilestoneDetail.MilestoneQuestions);
            var question = castedMilestoneDetail.MilestoneQuestions.First();
            Assert.Equal(1, question.MilestoneQuestionId);
            Assert.Equal(10, question.TeamMilestoneId);
            Assert.Equal(1, question.TeamId);
            Assert.Equal("What is this question about?", question.Question);
            Assert.Equal(13, question.AnswerCount);
            Assert.Equal(new DateTime(2025, 10, 25), question.CreatedTime);

            Assert.NotNull(castedMilestoneDetail.MilestoneEvaluation);
            var eval = castedMilestoneDetail.MilestoneEvaluation;
            Assert.Equal(10, eval.MilestoneId);
            Assert.Equal(1, eval.LecturerId);
            Assert.Equal("Fully returned every task results.", eval.Comment);
            Assert.Equal(1, eval.TeamId);
            Assert.Equal(87, eval.Score);
            Assert.Equal(new DateTime(2025, 12, 29), eval.CreatedDate);
            Assert.Equal(1, eval.LecturerId);
            Assert.Equal("LEC01", eval.LecturerCode);
            Assert.Equal("Lecturer1", eval.FullName);
            Assert.Equal("1234567890", eval.PhoneNumber);
            Assert.Equal("lecturer/avatar", eval.AvatarImg);

            Assert.Equal(2, castedMilestoneDetail.MilestoneFiles.Count);
            var file1 = castedMilestoneDetail.MilestoneFiles[0];
            var file2 = castedMilestoneDetail.MilestoneFiles[1];
            Assert.Equal(1, file1.FileId);
            Assert.Equal(2, file2.FileId);
            Assert.Equal(10, file1.TeamMilstoneId);
            Assert.Equal(10, file2.TeamMilstoneId);
            Assert.Equal("files/reports/doc1-example.docx", file1.FilePath);
            Assert.Equal("files/reports/mvp-checklist.docx", file2.FilePath);
            Assert.Equal("Word Doc", file1.Type);
            Assert.Equal("Word Doc", file2.Type);

            Assert.Equal(2, castedMilestoneDetail.MilestoneReturns.Count);
            var return1 = castedMilestoneDetail.MilestoneReturns[0];
            var return2 = castedMilestoneDetail.MilestoneReturns[1];
            Assert.Equal(1, return1.MileReturnId);
            Assert.Equal(2, return2.MileReturnId);
            Assert.Equal(3, return1.ClassMemberId);
            Assert.Equal(2, return2.ClassMemberId);
            Assert.Equal("files/return/mile1-context.xlse", return1.FilePath);
            Assert.Equal("files/return/mile1-doc-assign.xlse", return2.FilePath);
            Assert.Equal(new DateTime(2022, 1, 12), return1.SubmitedDate);
            Assert.Equal(new DateTime(2022, 2, 27), return2.SubmitedDate);
            Assert.Equal(3, return1.StudentId);
            Assert.Equal(1, return2.StudentId);
            Assert.Equal("Stu3", return1.Fullname);
            Assert.Equal("Stu1", return2.Fullname);
            Assert.Equal("SE1333", return1.StudentCode);
            Assert.Equal("SE1234", return2.StudentCode);
            Assert.Equal("avatar/student/avat_3.png", return1.AvatarImg);
            Assert.Equal("avatar/student/avat_1.png", return2.AvatarImg);
        }
    }
}
