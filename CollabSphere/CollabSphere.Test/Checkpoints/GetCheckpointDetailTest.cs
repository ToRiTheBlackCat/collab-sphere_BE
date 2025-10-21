using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Checkpoints;
using CollabSphere.Application.Features.Checkpoints.Queries.GetCheckpointDetail;
using CollabSphere.Application.Features.TeamMilestones.Queries.GetMilestoneDetail;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Checkpoints
{
    public class GetCheckpointDetailTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICheckpointRepository> _checkpointRepoMock;

        private readonly GetCheckpointDetailHandler _handler;

        public GetCheckpointDetailTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _checkpointRepoMock = new Mock<ICheckpointRepository>();

            _unitOfWorkMock.Setup(x => x.CheckpointRepo).Returns(_checkpointRepoMock.Object);

            _handler = new GetCheckpointDetailHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnCheckpoint_WhenValidQuery()
        {
            // Arrange
            var query = new GetCheckpointDetailQuery()
            {
                CheckpontId = 15,
                UserId = 8,
                UserRole = RoleConstants.LECTURER
            };

            var checkpoint = new Checkpoint()
            {
                CheckpointId = 15,
                TeamMilestoneId = 10,
                TeamMilestone = new TeamMilestone()
                {
                    TeamMilestoneId = 10,
                    Team = new Domain.Entities.Team()
                    {
                        TeamId = 7,
                        ClassId = 1,
                        LecturerId = 8,
                        ClassMembers = new List<ClassMember>()
                        {
                            new ClassMember()
                            {
                                ClassMemberId = 11,
                                StudentId = 11,
                            },
                        },
                    },
                },
            };

            _checkpointRepoMock.Setup(x => x.GetCheckpointDetail(15)).ReturnsAsync(checkpoint);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Checkpoint);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenNotMemberOfTeam()
        {
            // Arrange
            var query = new GetCheckpointDetailQuery()
            {
                CheckpontId = 15,
                UserId = 7,
                UserRole = RoleConstants.STUDENT
            };

            var checkpoint = new Checkpoint()
            {
                CheckpointId = 15,
                TeamMilestoneId = 10,
                TeamMilestone = new TeamMilestone()
                {
                    TeamMilestoneId = 10,
                    Team = new Domain.Entities.Team()
                    {
                        TeamId = 7,
                        ClassId = 1,
                        LecturerId = 8,
                        ClassMembers = new List<ClassMember>()
                        {
                            new ClassMember()
                            {
                                ClassMemberId = 11,
                                StudentId = 11,
                            },
                        },
                    },
                },
            };

            _checkpointRepoMock.Setup(x => x.GetCheckpointDetail(15)).ReturnsAsync(checkpoint);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("not a member of the team", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenNotLecturerOfClass()
        {
            // Arrange
            var query = new GetCheckpointDetailQuery()
            {
                CheckpontId = 15,
                UserId = 6,
                UserRole = RoleConstants.LECTURER
            };

            var checkpoint = new Checkpoint()
            {
                CheckpointId = 15,
                TeamMilestoneId = 10,
                TeamMilestone = new TeamMilestone()
                {
                    TeamMilestoneId = 10,
                    Team = new Domain.Entities.Team()
                    {
                        TeamId = 7,
                        ClassId = 1,
                        LecturerId = 8,
                        ClassMembers = new List<ClassMember>()
                        {
                            new ClassMember()
                            {
                                ClassMemberId = 11,
                                StudentId = 11,
                            },
                        },
                    },
                },
            };

            _checkpointRepoMock.Setup(x => x.GetCheckpointDetail(15)).ReturnsAsync(checkpoint);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("not the assigned lecturer of the class", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenCheckpointNotFound()
        {
            // Arrange
            var query = new GetCheckpointDetailQuery()
            {
                CheckpontId = 13,
                UserId = 8,
                UserRole = RoleConstants.LECTURER
            };

            var checkpoint = new Checkpoint()
            {
                CheckpointId = 15,
                TeamMilestoneId = 10,
                TeamMilestone = new TeamMilestone()
                {
                    TeamMilestoneId = 10,
                    Team = new Domain.Entities.Team()
                    {
                        TeamId = 7,
                        ClassId = 1,
                        LecturerId = 8,
                        ClassMembers = new List<ClassMember>()
                        {
                            new ClassMember()
                            {
                                ClassMemberId = 11,
                                StudentId = 11,
                            },
                        },
                    },
                },
            };

            _checkpointRepoMock.Setup(x => x.GetCheckpointDetail(15)).ReturnsAsync(checkpoint);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
            Assert.Null(result.Checkpoint);
        }

        [Fact]
        public void Cast_ShouldReturnCheckpoint_WhenValidEntity()
        {
            // Arrange
            var checkpoint = new Checkpoint()
            {
                CheckpointId = 15,
                TeamMilestoneId = 3,
                Title = "Checkpoint 1",
                Description = "Check first description.",
                Complexity = "HIGH",
                Status = (int)CheckpointStatuses.NOT_DONE,
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
                            StudentId = 1,
                            Student = new Student()
                            {
                                StudentId = 1,
                                Fullname = "Stu1",
                                StudentCode = "SE1234",
                                AvatarImg = "avatar/student/avat_1.png",
                            },
                        },
                    }
                },
                CheckpointFiles = new List<CheckpointFile>()
                {
                    new CheckpointFile()
                    {
                        FileId = 2,
                        CheckpointId = 15,
                        FilePath = "file/path_2.docx",
                        Type = "Word Doc"
                    }
                },
            };

            // Act
            var castedCheckpoint = (CheckpointDetailDto)checkpoint;

            // Assert
            Assert.Equal(15, castedCheckpoint.CheckpointId);
            Assert.Equal(3, castedCheckpoint.TeamMilestoneId);
            Assert.Equal("Checkpoint 1", castedCheckpoint.Title);
            Assert.Equal("Check first description.", castedCheckpoint.Description);
            Assert.Equal((int)CheckpointStatuses.NOT_DONE, castedCheckpoint.Status);
            Assert.Equal(new DateOnly(2021, 2, 12), castedCheckpoint.StartDate);
            Assert.Equal(new DateOnly(2021, 2, 20), castedCheckpoint.DueDate);

            Assert.Single(castedCheckpoint.CheckpointAssignments);
            var assignment = castedCheckpoint.CheckpointAssignments.First();
            Assert.Equal(1, assignment.CheckpointAssignmentId);
            Assert.Equal(2, assignment.ClassMemberId);
            Assert.Equal((int)TeamRole.LEADER, assignment.TeamRole);
            Assert.Equal(1, assignment.StudentId);
            Assert.Equal("Stu1", assignment.Fullname);
            Assert.Equal("SE1234", assignment.StudentCode);
            Assert.Equal("avatar/student/avat_1.png", assignment.AvatarImg);

            Assert.Single(castedCheckpoint.CheckpointFiles);
            var file = castedCheckpoint.CheckpointFiles.First();
            Assert.Equal(2, file.FileId);
            Assert.Equal(15, file.CheckpointId);
            Assert.Equal("file/path_2.docx", file.FilePath);
            Assert.Equal("Word Doc", file.Type);
        }
    }
}
