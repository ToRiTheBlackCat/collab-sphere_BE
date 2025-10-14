using CollabSphere.Application;
using CollabSphere.Application.DTOs.Objective;
using CollabSphere.Application.Features.Project.Commands.ApproveProject;
using CollabSphere.Application.Features.Project.Commands.UpdateProject;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Projects
{
    public class UpdateProjectHandlerTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;
        private readonly Mock<IObjectiveRepository> _objectiveRepoMock;
        private readonly Mock<IObjectiveMilestoneRepository> _milestoneRepoMock;
        private readonly Mock<ILecturerRepository> _lecturerRepoMock;
        private readonly Mock<ISubjectRepository> _subjectRepoMock;
        

        private readonly UpdateProjectHandler _handler;

        public UpdateProjectHandlerTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _projectRepoMock = new Mock<IProjectRepository>();
            _objectiveRepoMock = new Mock<IObjectiveRepository>();
            _milestoneRepoMock = new Mock<IObjectiveMilestoneRepository>();
            _lecturerRepoMock = new Mock<ILecturerRepository>();
            _subjectRepoMock = new Mock<ISubjectRepository>();

            _unitOfWorkMock.Setup(x => x.ProjectRepo).Returns(_projectRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.ObjectiveRepo).Returns(_objectiveRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.ObjectiveMilestoneRepo).Returns(_milestoneRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.LecturerRepo).Returns(_lecturerRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.SubjectRepo).Returns(_subjectRepoMock.Object);

            _handler = new UpdateProjectHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldUpdateProject_WhenValidCommand()
        {
            // Arrange
            var command = new UpdateProjectCommand()
            {
                Project = new Application.DTOs.Project.UpdateProjectDTO()
                {
                    ProjectId = 2,
                    ProjectName = "Update Project Name",
                    Description = "Description for Updated Project.",
                    SubjectId = 1,
                    Objectives = new List<UpdateProjectObjectiveDTO>()
                    {
                        new UpdateProjectObjectiveDTO()
                        {
                            ObjectiveId = 0,
                            Description = "Existing Objective",
                            Priority = "Medium",
                            ObjectiveMilestones = new List<Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO>()
                            {
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 0,
                                    Title = "Milestone1",
                                    Description = "Milestone1 Description",
                                    StartDate = new DateOnly(2025, 10, 3),
                                    EndDate = new DateOnly(2025, 10, 8),
                                },
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 0,
                                    Title = "Milestone2",
                                    Description = "Milestone2 deep dive",
                                    StartDate = new DateOnly(2025, 10, 9),
                                    EndDate = new DateOnly(2025, 10, 12),
                                } 
                            }
                        }
                    }
                },
                UserId = 8,
            };

            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Project() { ProjectId = 2, LecturerId = 8 });
            _lecturerRepoMock.Setup(x => x.GetById(8)).ReturnsAsync(new Lecturer() { LecturerId = 8 });
            _subjectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Subject() { SubjectId = 1 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            _projectRepoMock.Verify(x => x.Update(It.Is<Project>(x => x.ProjectName == "Update Project Name" && x.Objectives.Count == 1 && x.Objectives.First().ObjectiveMilestones.Count == 2)), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldDeleteExistingObjectives_WhenObjectiveIdNotInCommand()
        {
            // Arrange
            var command = new UpdateProjectCommand()
            {
                Project = new Application.DTOs.Project.UpdateProjectDTO()
                {
                    ProjectId = 2,
                    ProjectName = "Update Project Name",
                    Description = "Description for Updated Project.",
                    SubjectId = 1,
                    Objectives = new List<UpdateProjectObjectiveDTO>()
                    {
                        new UpdateProjectObjectiveDTO()
                        {
                            ObjectiveId = 0,
                            Description = "Existing Objective",
                            Priority = "Medium",
                            ObjectiveMilestones = new List<Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO>()
                            {
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 0,
                                    Title = "Milestone1",
                                    Description = "Milestone1 Description",
                                    StartDate = new DateOnly(2025, 10, 3),
                                    EndDate = new DateOnly(2025, 10, 8),
                                },
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 0,
                                    Title = "Milestone2",
                                    Description = "Milestone2 deep dive",
                                    StartDate = new DateOnly(2025, 10, 9),
                                    EndDate = new DateOnly(2025, 10, 12),
                                }
                            }
                        }
                    }
                },
                UserId = 8,
            };

            var objectives = new List<Objective>()
            {
                new Objective()
                {
                    ObjectiveId = 5,
                    ProjectId = 2,
                    Description = "Objective to delete.",
                }
            };

            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Project() { ProjectId = 2, LecturerId = 8, Objectives = objectives });
            _lecturerRepoMock.Setup(x => x.GetById(8)).ReturnsAsync(new Lecturer() { LecturerId = 8 });
            _subjectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Subject() { SubjectId = 1 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            _projectRepoMock.Verify(x => x.Update(It.Is<Project>(x => x.ProjectName == "Update Project Name" && x.Objectives.Count == 1 && x.Objectives.First().ObjectiveMilestones.Count == 2)), Times.Once);
            _objectiveRepoMock.Verify(x => x.Delete(It.Is<Objective>(x => x.ObjectiveId == 5)), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldDeleteExistingMilestones_WhenMilestoneIdsNotInCommand()
        {
            // Arrange
            var command = new UpdateProjectCommand()
            {
                Project = new Application.DTOs.Project.UpdateProjectDTO()
                {
                    ProjectId = 2,
                    ProjectName = "Update Project Name",
                    Description = "Description for Updated Project.",
                    SubjectId = 1,
                    Objectives = new List<UpdateProjectObjectiveDTO>()
                    {
                        new UpdateProjectObjectiveDTO()
                        {
                            ObjectiveId = 1,
                            Description = "Existing Objective",
                            Priority = "Medium",
                            ObjectiveMilestones = new List<Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO>()
                            {
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 0,
                                    Title = "Milestone1",
                                    Description = "Milestone1 Description",
                                    StartDate = new DateOnly(2025, 10, 3),
                                    EndDate = new DateOnly(2025, 10, 8),
                                },
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 0,
                                    Title = "Milestone2",
                                    Description = "Milestone2 deep dive",
                                    StartDate = new DateOnly(2025, 10, 9),
                                    EndDate = new DateOnly(2025, 10, 12),
                                }
                            }
                        }
                    }
                },
                UserId = 8,
            };

            var objectives = new List<Objective>()
            {
                new Objective()
                {
                    ObjectiveId = 1,
                    ProjectId = 2,
                    Description = "Existing Objective.",
                    ObjectiveMilestones = new List<ObjectiveMilestone>()
                    {   
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 5,
                            ObjectiveId = 1,
                            Description = "Milestone to delete."
                        }
                    } 
                }
            };

            var capturedProject = new Project();

            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Project() { ProjectId = 2, LecturerId = 8, Objectives = objectives });
            _lecturerRepoMock.Setup(x => x.GetById(8)).ReturnsAsync(new Lecturer() { LecturerId = 8 });
            _subjectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Subject() { SubjectId = 1 });
            _projectRepoMock.Setup(x => x.Update(It.IsAny<Project>())).Callback<Project>(x => capturedProject = x);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            _projectRepoMock.Verify(x => x.Update(It.Is<Project>(x => x.ProjectName == "Update Project Name" && x.Objectives.Count == 1 && x.Objectives.First().ObjectiveMilestones.Count == 2)), Times.Once);
            _milestoneRepoMock.Verify(x => x.Delete(It.Is<ObjectiveMilestone>(x => x.ObjectiveMilestoneId == 5)), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShoulUpdateExistingObjectives_WhenObjectiveIdInCommand()
        {
            // Arrange
            var command = new UpdateProjectCommand()
            {
                Project = new Application.DTOs.Project.UpdateProjectDTO()
                {
                    ProjectId = 2,
                    ProjectName = "Update Project Name",
                    Description = "Description for Updated Project.",
                    SubjectId = 1,
                    Objectives = new List<UpdateProjectObjectiveDTO>()
                    {
                        new UpdateProjectObjectiveDTO()
                        {
                            ObjectiveId = 5,
                            Description = "Objective Updated.",
                            Priority = "Medium",
                            ObjectiveMilestones = new List<Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO>()
                            {
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 0,
                                    Title = "Milestone1",
                                    Description = "Milestone1 Description",
                                    StartDate = new DateOnly(2025, 10, 3),
                                    EndDate = new DateOnly(2025, 10, 8),
                                },
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 0,
                                    Title = "Milestone2",
                                    Description = "Milestone2 deep dive",
                                    StartDate = new DateOnly(2025, 10, 9),
                                    EndDate = new DateOnly(2025, 10, 12),
                                }
                            }
                        }
                    }
                },
                UserId = 8,
            };

            var objectives = new List<Objective>()
            {
                new Objective()
                {
                    ObjectiveId = 5,
                    ProjectId = 2,
                    Description = "Objective to Update.",
                }
            };

            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Project() { ProjectId = 2, LecturerId = 8, Objectives = objectives });
            _lecturerRepoMock.Setup(x => x.GetById(8)).ReturnsAsync(new Lecturer() { LecturerId = 8 });
            _subjectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Subject() { SubjectId = 1 });

            var capturedObjective = new Objective();
            _projectRepoMock.Setup(x => x.Update(It.IsAny<Project>())).Callback<Project>(x => capturedObjective = x.Objectives.First());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            _projectRepoMock.Verify(x => x.Update(It.Is<Project>(x => x.ProjectName == "Update Project Name" && x.Objectives.Count == 1 && x.Objectives.First().ObjectiveMilestones.Count == 2)), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);

            Assert.Equal(5, capturedObjective.ObjectiveId);
            Assert.Equal(2, capturedObjective.ProjectId);
            Assert.Equal("Objective Updated.", capturedObjective.Description);
            Assert.Equal(2, capturedObjective.ObjectiveMilestones.Count);
        }

        [Fact]
        public async Task Handle_ShoulUpdateExistingMilestone_WhenMilestoneIdInCommand()
        {
            // Arrange
            var command = new UpdateProjectCommand()
            {
                Project = new Application.DTOs.Project.UpdateProjectDTO()
                {
                    ProjectId = 2,
                    ProjectName = "Update Project Name",
                    Description = "Description for Updated Project.",
                    SubjectId = 1,
                    Objectives = new List<UpdateProjectObjectiveDTO>()
                    {
                        new UpdateProjectObjectiveDTO()
                        {
                            ObjectiveId = 5,
                            Description = "Objective Updated.",
                            Priority = "Medium",
                            ObjectiveMilestones = new List<Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO>()
                            {
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 12,
                                    Title = "Milestone1",
                                    Description = "Milestone1 Updated Desc.",
                                    StartDate = new DateOnly(2025, 10, 3),
                                    EndDate = new DateOnly(2025, 10, 8),
                                },
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 13,
                                    Title = "Milestone2 updated",
                                    Description = "Milestone2 longer desc.",
                                    StartDate = new DateOnly(2025, 10, 9),
                                    EndDate = new DateOnly(2025, 10, 12),
                                }
                            }
                        }
                    }
                },
                UserId = 8,
            };

            var objectives = new List<Objective>()
            {
                new Objective()
                {
                    ObjectiveId = 5,
                    ProjectId = 2,
                    Description = "Objective to Update.",
                    ObjectiveMilestones = new List<ObjectiveMilestone>()
                    {
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 12,
                            Title = "Milestone1",
                            Description = "Milestone1 Description.",
                        },
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 13,
                            Title = "Milestone2",
                            Description = "Milestone2 deep dive.",
                        },
                    }
                }
            };

            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Project() { ProjectId = 2, LecturerId = 8, Objectives = objectives });
            _lecturerRepoMock.Setup(x => x.GetById(8)).ReturnsAsync(new Lecturer() { LecturerId = 8 });
            _subjectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Subject() { SubjectId = 1 });

            var capturedObjective = new Objective();
            _projectRepoMock.Setup(x => x.Update(It.IsAny<Project>())).Callback<Project>(x => capturedObjective = x.Objectives.First());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            _projectRepoMock.Verify(x => x.Update(It.Is<Project>(x => x.ProjectName == "Update Project Name" && x.Objectives.Count == 1 && x.Objectives.First().ObjectiveMilestones.Count == 2)), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);

            Assert.Equal(2, capturedObjective.ObjectiveMilestones.Count);
            Assert.Equal("Milestone1 Updated Desc.", capturedObjective.ObjectiveMilestones.ElementAt(0).Description);
            Assert.Equal("Milestone2 longer desc.", capturedObjective.ObjectiveMilestones.ElementAt(1).Description);
            Assert.Equal("Milestone2 updated", capturedObjective.ObjectiveMilestones.ElementAt(1).Title);
        }

        [Fact]
        public async Task Handle_ShoulFailValidation_WhenLecturerNotFound()
        {
            // Arrange
            var command = new UpdateProjectCommand()
            {
                Project = new Application.DTOs.Project.UpdateProjectDTO()
                {
                    ProjectId = 2,
                    ProjectName = "Update Project Name",
                    Description = "Description for Updated Project.",
                    SubjectId = 1,
                    Objectives = new List<UpdateProjectObjectiveDTO>()
                    {
                        new UpdateProjectObjectiveDTO()
                        {
                            ObjectiveId = 5,
                            Description = "Objective Updated.",
                            Priority = "Medium",
                            ObjectiveMilestones = new List<Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO>()
                            {
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 12,
                                    Title = "Milestone1",
                                    Description = "Milestone1 Updated Desc.",
                                    StartDate = new DateOnly(2025, 10, 3),
                                    EndDate = new DateOnly(2025, 10, 8),
                                },
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 13,
                                    Title = "Milestone2 updated",
                                    Description = "Milestone2 longer desc.",
                                    StartDate = new DateOnly(2025, 10, 9),
                                    EndDate = new DateOnly(2025, 10, 12),
                                }
                            }
                        }
                    }
                },
                UserId = 8,
            };

            var objectives = new List<Objective>()
            {
                new Objective()
                {
                    ObjectiveId = 5,
                    ProjectId = 2,
                    Description = "Objective to Update.",
                    ObjectiveMilestones = new List<ObjectiveMilestone>()
                    {
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 12,
                            Title = "Milestone1",
                            Description = "Milestone1 Description.",
                        },
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 13,
                            Title = "Milestone2",
                            Description = "Milestone2 deep dive.",
                        },
                    }
                }
            };

            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Project() { ProjectId = 2, LecturerId = 8, Objectives = objectives });
            //_lecturerRepoMock.Setup(x => x.GetById(8)).ReturnsAsync(new Lecturer() { LecturerId = 8 });
            _subjectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Subject() { SubjectId = 1 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("Lecturer with this ID", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShoulFailValidation_WhenSubejctNotFound()
        {
            // Arrange
            var command = new UpdateProjectCommand()
            {
                Project = new Application.DTOs.Project.UpdateProjectDTO()
                {
                    ProjectId = 2,
                    ProjectName = "Update Project Name",
                    Description = "Description for Updated Project.",
                    SubjectId = 1,
                    Objectives = new List<UpdateProjectObjectiveDTO>()
                    {
                        new UpdateProjectObjectiveDTO()
                        {
                            ObjectiveId = 5,
                            Description = "Objective Updated.",
                            Priority = "Medium",
                            ObjectiveMilestones = new List<Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO>()
                            {
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 12,
                                    Title = "Milestone1",
                                    Description = "Milestone1 Updated Desc.",
                                    StartDate = new DateOnly(2025, 10, 3),
                                    EndDate = new DateOnly(2025, 10, 8),
                                },
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 13,
                                    Title = "Milestone2 updated",
                                    Description = "Milestone2 longer desc.",
                                    StartDate = new DateOnly(2025, 10, 9),
                                    EndDate = new DateOnly(2025, 10, 12),
                                }
                            }
                        }
                    }
                },
                UserId = 8,
            };

            var objectives = new List<Objective>()
            {
                new Objective()
                {
                    ObjectiveId = 5,
                    ProjectId = 2,
                    Description = "Objective to Update.",
                    ObjectiveMilestones = new List<ObjectiveMilestone>()
                    {
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 12,
                            Title = "Milestone1",
                            Description = "Milestone1 Description.",
                        },
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 13,
                            Title = "Milestone2",
                            Description = "Milestone2 deep dive.",
                        },
                    }
                }
            };

            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Project() { ProjectId = 2, LecturerId = 8, Objectives = objectives });
            _lecturerRepoMock.Setup(x => x.GetById(8)).ReturnsAsync(new Lecturer() { LecturerId = 8 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("Subject with this ID", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShoulFailValidation_MilestoneStartDateIsAfterEndDate()
        {
            // Arrange
            var command = new UpdateProjectCommand()
            {
                Project = new Application.DTOs.Project.UpdateProjectDTO()
                {
                    ProjectId = 2,
                    ProjectName = "Update Project Name",
                    Description = "Description for Updated Project.",
                    SubjectId = 1,
                    Objectives = new List<UpdateProjectObjectiveDTO>()
                    {
                        new UpdateProjectObjectiveDTO()
                        {
                            ObjectiveId = 5,
                            Description = "Objective Updated.",
                            Priority = "Medium",
                            ObjectiveMilestones = new List<Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO>()
                            {
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 12,
                                    Title = "Milestone1",
                                    Description = "Milestone1 Updated Desc.",
                                    StartDate = new DateOnly(2025, 10, 9),
                                    EndDate = new DateOnly(2025, 10, 8),
                                },
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 13,
                                    Title = "Milestone2 updated",
                                    Description = "Milestone2 longer desc.",
                                    StartDate = new DateOnly(2025, 10, 9),
                                    EndDate = new DateOnly(2025, 10, 12),
                                }
                            }
                        }
                    }
                },
                UserId = 8,
            };

            var objectives = new List<Objective>()
            {
                new Objective()
                {
                    ObjectiveId = 5,
                    ProjectId = 2,
                    Description = "Objective to Update.",
                    ObjectiveMilestones = new List<ObjectiveMilestone>()
                    {
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 12,
                            Title = "Milestone1",
                            Description = "Milestone1 Description.",
                        },
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 13,
                            Title = "Milestone2",
                            Description = "Milestone2 deep dive.",
                        },
                    }
                }
            };

            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Project() { ProjectId = 2, LecturerId = 8, Objectives = objectives });
            _lecturerRepoMock.Setup(x => x.GetById(8)).ReturnsAsync(new Lecturer() { LecturerId = 8 });
            _subjectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Subject() { SubjectId = 1 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("2 days after StartDate", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShoulFailValidation_MilestoneStartDateBeforePreviousMilestoneEndDate()
        {
            // Arrange
            var command = new UpdateProjectCommand()
            {
                Project = new Application.DTOs.Project.UpdateProjectDTO()
                {
                    ProjectId = 2,
                    ProjectName = "Update Project Name",
                    Description = "Description for Updated Project.",
                    SubjectId = 1,
                    Objectives = new List<UpdateProjectObjectiveDTO>()
                    {
                        new UpdateProjectObjectiveDTO()
                        {
                            ObjectiveId = 5,
                            Description = "Objective Updated.",
                            Priority = "Medium",
                            ObjectiveMilestones = new List<Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO>()
                            {
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 12,
                                    Title = "Milestone1",
                                    Description = "Milestone1 Updated Desc.",
                                    StartDate = new DateOnly(2025, 10, 3),
                                    EndDate = new DateOnly(2025, 10, 8),
                                },
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 13,
                                    Title = "Milestone2 updated",
                                    Description = "Milestone2 longer desc.",
                                    StartDate = new DateOnly(2025, 10, 7),
                                    EndDate = new DateOnly(2025, 10, 12),
                                }
                            }
                        }
                    }
                },
                UserId = 8,
            };

            var objectives = new List<Objective>()
            {
                new Objective()
                {
                    ObjectiveId = 5,
                    ProjectId = 2,
                    Description = "Objective to Update.",
                    ObjectiveMilestones = new List<ObjectiveMilestone>()
                    {
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 12,
                            Title = "Milestone1",
                            Description = "Milestone1 Description.",
                        },
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 13,
                            Title = "Milestone2",
                            Description = "Milestone2 deep dive.",
                        },
                    }
                }
            };

            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Project() { ProjectId = 2, LecturerId = 8, Objectives = objectives });
            _lecturerRepoMock.Setup(x => x.GetById(8)).ReturnsAsync(new Lecturer() { LecturerId = 8 });
            _subjectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Subject() { SubjectId = 1 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("Milestone start date cannot be before the previous", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShoulFailValidation_WhenObjectiveFirstStartDateIsBeforePreviousObjectiveEndDate()
        {
            // Arrange
            var command = new UpdateProjectCommand()
            {
                Project = new Application.DTOs.Project.UpdateProjectDTO()
                {
                    ProjectId = 2,
                    ProjectName = "Update Project Name",
                    Description = "Description for Updated Project.",
                    SubjectId = 1,
                    Objectives = new List<UpdateProjectObjectiveDTO>()
                    {
                        new UpdateProjectObjectiveDTO()
                        {
                            ObjectiveId = 5,
                            Description = "Objective Updated.",
                            Priority = "Medium",
                            ObjectiveMilestones = new List<Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO>()
                            {
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 12,
                                    Title = "Milestone1",
                                    Description = "Milestone1 Updated Desc.",
                                    StartDate = new DateOnly(2025, 10, 3),
                                    EndDate = new DateOnly(2025, 10, 8),
                                },
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 13,
                                    Title = "Milestone2 updated",
                                    Description = "Milestone2 longer desc.",
                                    StartDate = new DateOnly(2025, 10, 9),
                                    EndDate = new DateOnly(2025, 10, 12),
                                }
                            }
                        },
                        new UpdateProjectObjectiveDTO()
                        {
                            ObjectiveId = 0,
                            Description = "Objective 2",
                            Priority = "Medium",
                            ObjectiveMilestones = new List<Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO>()
                            {
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 0,
                                    Title = "Begin Obj2",
                                    Description = "Milestone new Description",
                                    StartDate = new DateOnly(2025, 10, 11),
                                    EndDate = new DateOnly(2025, 11, 2),
                                },
                            }
                        }
                    }
                },
                UserId = 8,
            };

            var objectives = new List<Objective>()
            {
                new Objective()
                {
                    ObjectiveId = 5,
                    ProjectId = 2,
                    Description = "Objective to Update.",
                    ObjectiveMilestones = new List<ObjectiveMilestone>()
                    {
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 12,
                            Title = "Milestone1",
                            Description = "Milestone1 Description.",
                        },
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 13,
                            Title = "Milestone2",
                            Description = "Milestone2 deep dive.",
                        },
                    }
                }
            };

            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Project() { ProjectId = 2, LecturerId = 8, Objectives = objectives });
            _lecturerRepoMock.Setup(x => x.GetById(8)).ReturnsAsync(new Lecturer() { LecturerId = 8 });
            _subjectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Subject() { SubjectId = 1 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("Objective's start date cannot be before the previous", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShoulFailValidation_WhenObjectiveNotExist()
        {
            // Arrange
            var command = new UpdateProjectCommand()
            {
                Project = new Application.DTOs.Project.UpdateProjectDTO()
                {
                    ProjectId = 2,
                    ProjectName = "Update Project Name",
                    Description = "Description for Updated Project.",
                    SubjectId = 1,
                    Objectives = new List<UpdateProjectObjectiveDTO>()
                    {
                        new UpdateProjectObjectiveDTO()
                        {
                            ObjectiveId = 5,
                            Description = "Objective Updated.",
                            Priority = "Medium",
                            ObjectiveMilestones = new List<Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO>()
                            {
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 12,
                                    Title = "Milestone1",
                                    Description = "Milestone1 Updated Desc.",
                                    StartDate = new DateOnly(2025, 10, 3),
                                    EndDate = new DateOnly(2025, 10, 8),
                                },
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 13,
                                    Title = "Milestone2 updated",
                                    Description = "Milestone2 longer desc.",
                                    StartDate = new DateOnly(2025, 10, 9),
                                    EndDate = new DateOnly(2025, 10, 12),
                                }
                            }
                        },
                        new UpdateProjectObjectiveDTO()
                        {
                            ObjectiveId = 12,
                            Description = "Objective 2",
                            Priority = "Medium",
                            ObjectiveMilestones = new List<Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO>()
                            {
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 0,
                                    Title = "Begin Obj2",
                                    Description = "Milestone new Description",
                                    StartDate = new DateOnly(2025, 10, 11),
                                    EndDate = new DateOnly(2025, 11, 2),
                                },
                            }
                        }
                    }
                },
                UserId = 8,
            };

            var objectives = new List<Objective>()
            {
                new Objective()
                {
                    ObjectiveId = 5,
                    ProjectId = 2,
                    Description = "Objective to Update.",
                    ObjectiveMilestones = new List<ObjectiveMilestone>()
                    {
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 12,
                            Title = "Milestone1",
                            Description = "Milestone1 Description.",
                        },
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 13,
                            Title = "Milestone2",
                            Description = "Milestone2 deep dive.",
                        },
                    }
                }
            };

            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Project() { ProjectId = 2, LecturerId = 8, Objectives = objectives });
            _lecturerRepoMock.Setup(x => x.GetById(8)).ReturnsAsync(new Lecturer() { LecturerId = 8 });
            _subjectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Subject() { SubjectId = 1 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("doesn't have any Objective with ID", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShoulFailValidation_WhenMilestoneNotExist()
        {
            // Arrange
            var command = new UpdateProjectCommand()
            {
                Project = new Application.DTOs.Project.UpdateProjectDTO()
                {
                    ProjectId = 2,
                    ProjectName = "Update Project Name",
                    Description = "Description for Updated Project.",
                    SubjectId = 1,
                    Objectives = new List<UpdateProjectObjectiveDTO>()
                    {
                        new UpdateProjectObjectiveDTO()
                        {
                            ObjectiveId = 5,
                            Description = "Objective Updated.",
                            Priority = "Medium",
                            ObjectiveMilestones = new List<Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO>()
                            {
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 12,
                                    Title = "Milestone1",
                                    Description = "Milestone1 Updated Desc.",
                                    StartDate = new DateOnly(2025, 10, 3),
                                    EndDate = new DateOnly(2025, 10, 8),
                                },
                                new Application.DTOs.ObjectiveMilestone.UpdateProjectObjectiveMilestoneDTO()
                                {
                                    ObjectiveMilestoneId = 14,
                                    Title = "Milestone2 updated",
                                    Description = "Milestone2 longer desc.",
                                    StartDate = new DateOnly(2025, 10, 9),
                                    EndDate = new DateOnly(2025, 10, 12),
                                }
                            }
                        }
                    }
                },
                UserId = 8,
            };

            var objectives = new List<Objective>()
            {
                new Objective()
                {
                    ObjectiveId = 5,
                    ProjectId = 2,
                    Description = "Objective to Update.",
                    ObjectiveMilestones = new List<ObjectiveMilestone>()
                    {
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 12,
                            Title = "Milestone1",
                            Description = "Milestone1 Description.",
                        },
                        new ObjectiveMilestone()
                        {
                            ObjectiveMilestoneId = 13,
                            Title = "Milestone2",
                            Description = "Milestone2 deep dive.",
                        },
                    }
                }
            };

            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Project() { ProjectId = 2, LecturerId = 8, Objectives = objectives });
            _lecturerRepoMock.Setup(x => x.GetById(8)).ReturnsAsync(new Lecturer() { LecturerId = 8 });
            _subjectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Subject() { SubjectId = 1 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("doesn't have any Milestone with ID", result.ErrorList.First().Message);
        }
    }
}
