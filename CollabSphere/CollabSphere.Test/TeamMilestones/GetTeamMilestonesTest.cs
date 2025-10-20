using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Project.Commands.ApproveProject;
using CollabSphere.Application.Features.Team.Queries.GetTeamDetail;
using CollabSphere.Application.Features.TeamMilestones.Queries.GetMilestonesByTeam;
using CollabSphere.Application.Mappings.TeamMilestones;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.TeamMilestones
{
    public class GetTeamMilestonesTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITeamRepository> _teamRepoMock;
        private readonly Mock<ITeamMilestoneRepository> _teamMilestoneRepoMock;

        private readonly GetMilestonesByTeamHandler _handler;

        public GetTeamMilestonesTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _teamRepoMock = new Mock<ITeamRepository>();
            _teamMilestoneRepoMock = new Mock<ITeamMilestoneRepository>();

            _unitOfWorkMock.Setup(x => x.TeamRepo).Returns(_teamRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.TeamMilestoneRepo).Returns(_teamMilestoneRepoMock.Object);

            _handler = new GetMilestonesByTeamHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnMilestones_WhenValidQuery()
        {
            // Arrange
            var query = new GetMilestonesByTeamQuery()
            {
                TeamId = 1,
                UserId = 3,
                UserRole = (int)RoleConstants.STUDENT,
            };

            var teamEntity = new Domain.Entities.Team()
            {
                TeamId = 1,
                ClassMembers = new List<ClassMember>()
                {
                    new ClassMember()
                    {
                        ClassMemberId = 1,
                        StudentId = 3,
                    }
                },
            };

            var teamMilestones = new List<TeamMilestone>()
            {
                new TeamMilestone()
                {
                    TeamMilestoneId = 1,
                    Title = "Milestone 1",
                    Description = "Desc 1",
                    Checkpoints = new List<Checkpoint>()
                    {
                        new Checkpoint()
                        {
                            CheckpointId = 1,
                        },
                        new Checkpoint()
                        {
                            CheckpointId = 2,
                        }
                    }
                },
                new TeamMilestone()
                {
                    TeamMilestoneId = 2,
                    Title = "Milestone 2",
                    Description = "Desc 2",
                },
            };

            _teamRepoMock.Setup(x => x.GetTeamDetail(1)).ReturnsAsync(teamEntity);
            _teamMilestoneRepoMock.Setup(x => x.GetMilestonesByTeamId(1)).ReturnsAsync(teamMilestones);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.TeamMilestones.Count);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenNotLecturerOfClass()
        {
            // Arrange
            var query = new GetMilestonesByTeamQuery()
            {
                TeamId = 1,
                UserId = 16,
                UserRole = RoleConstants.LECTURER,
            };

            var teamEntity = new Domain.Entities.Team()
            {
                TeamId = 1,
                ClassMembers = new List<ClassMember>()
                {
                    new ClassMember()
                    {
                        ClassMemberId = 1,
                        StudentId = 3,
                    }
                },
                Class = new Class()
                {
                    ClassId = 12,
                    LecturerId = 8,
                }
            };

            var teamMilestones = new List<TeamMilestone>()
            {
                new TeamMilestone()
                {
                    TeamMilestoneId = 1,
                    Title = "Milestone 1",
                    Description = "Desc 1",
                    Checkpoints = new List<Checkpoint>()
                    {
                        new Checkpoint()
                        {
                            CheckpointId = 1,
                        },
                        new Checkpoint()
                        {
                            CheckpointId = 2,
                        }
                    }
                },
                new TeamMilestone()
                {
                    TeamMilestoneId = 2,
                    Title = "Milestone 2",
                    Description = "Desc 2",
                },
            };

            _teamRepoMock.Setup(x => x.GetTeamDetail(1)).ReturnsAsync(teamEntity);
            _teamMilestoneRepoMock.Setup(x => x.GetMilestonesByTeamId(1)).ReturnsAsync(teamMilestones);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("not the lecturer of the class", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenStudentNotMemberOfTeam()
        {
            // Arrange
            var query = new GetMilestonesByTeamQuery()
            {
                TeamId = 1,
                UserId = 4,
                UserRole = RoleConstants.STUDENT,
            };

            var teamEntity = new Domain.Entities.Team()
            {
                TeamId = 1,
                ClassMembers = new List<ClassMember>()
                {
                    new ClassMember()
                    {
                        ClassMemberId = 1,
                        StudentId = 3,
                    }
                },
                Class = new Class()
                {
                    ClassId = 12,
                    LecturerId = 8,
                }
            };

            var teamMilestones = new List<TeamMilestone>()
            {
                new TeamMilestone()
                {
                    TeamMilestoneId = 1,
                    Title = "Milestone 1",
                    Description = "Desc 1",
                    Checkpoints = new List<Checkpoint>()
                    {
                        new Checkpoint()
                        {
                            CheckpointId = 1,
                        },
                        new Checkpoint()
                        {
                            CheckpointId = 2,
                        }
                    }
                },
                new TeamMilestone()
                {
                    TeamMilestoneId = 2,
                    Title = "Milestone 2",
                    Description = "Desc 2",
                },
            };

            _teamRepoMock.Setup(x => x.GetTeamDetail(1)).ReturnsAsync(teamEntity);
            _teamMilestoneRepoMock.Setup(x => x.GetMilestonesByTeamId(1)).ReturnsAsync(teamMilestones);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("not a member of the team", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenTeamNotFound()
        {
            // Arrange
            var query = new GetMilestonesByTeamQuery()
            {
                TeamId = 1,
                UserId = 4,
                UserRole = RoleConstants.STUDENT,
            };

            var teamMilestones = new List<TeamMilestone>()
            {
                new TeamMilestone()
                {
                    TeamMilestoneId = 1,
                    Title = "Milestone 1",
                    Description = "Desc 1",
                    Checkpoints = new List<Checkpoint>()
                    {
                        new Checkpoint()
                        {
                            CheckpointId = 1,
                        },
                        new Checkpoint()
                        {
                            CheckpointId = 2,
                        }
                    }
                },
                new TeamMilestone()
                {
                    TeamMilestoneId = 2,
                    Title = "Milestone 2",
                    Description = "Desc 2",
                },
            };
            _teamMilestoneRepoMock.Setup(x => x.GetMilestonesByTeamId(1)).ReturnsAsync(teamMilestones);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("No team with ID", result.ErrorList.First().Message);
        }

        [Fact]
        public void Handle_CastingShouldBindProperties()
        {
            // Arrange
            var teamMilestones = new List<TeamMilestone>()
            {
                new TeamMilestone()
                {
                    TeamMilestoneId = 1,
                    TeamId = 2,
                    ObjectiveMilestoneId = 3,
                    Title = "Milestone 1",
                    Description = "Desc 1",
                    StartDate = new DateOnly(2021,2,12),
                    EndDate = new DateOnly(2021,3,21),
                    Progress = 12,
                    Status = (int)TeamMilestoneStatuses.NOT_DONE,
                    Checkpoints = new List<Checkpoint>()
                    {
                        new Checkpoint()
                        {
                            CheckpointId = 1,
                        },
                        new Checkpoint()
                        {
                            CheckpointId = 2,
                        }
                    },
                    MilestoneQuestions = new List<MilestoneQuestion>()
                    {
                        new MilestoneQuestion()
                        {
                            MilestoneQuestionId = 1,
                        },
                    }
                },
                new TeamMilestone()
                {
                    TeamMilestoneId = 2,
                    Title = "Milestone 2",
                    Description = "Desc 2",
                },
            };

            // Act
            var castedEntities = teamMilestones.ToTeamMilestoneVM();

            // Assert
            Assert.Equal(2, castedEntities.Count);

            var milestone = castedEntities[0];
            Assert.Equal(1, milestone.TeamMilestoneId);
            Assert.Equal(2, milestone.TeamId);
            Assert.Equal(3, milestone.ObjectiveMilestoneId);
            Assert.Equal("Milestone 1", milestone.Title);
            Assert.Equal(new DateOnly(2021, 2, 12), milestone.StartDate);
            Assert.Equal(new DateOnly(2021, 3, 21), milestone.EndDate);
            Assert.Equal(12, milestone.Progress);
            Assert.Equal((int)TeamMilestoneStatuses.NOT_DONE, milestone.Status);
            Assert.Equal(2, milestone.CheckpointCount);
            Assert.Equal(1, milestone.MilestoneQuestionCount);
        }
    }
}
