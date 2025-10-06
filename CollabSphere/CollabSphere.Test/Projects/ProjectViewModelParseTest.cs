using CollabSphere.Application.DTOs.Project;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Projects
{
    public class ProjectViewModelParseTest
    {
        public ProjectViewModelParseTest()
        {

        }

        [Fact]
        public async Task Cast_ShouldMapCorrectly_ValidObject()
        {
            // Arrange
            var project = new Project()
            {
                ProjectId = 1,
                ProjectName = "Supplies Exchange Web App",
                Description = "A web app for student to exchange school supplies",
                LecturerId = 1,
                Lecturer = new Lecturer()
                {
                    Fullname = "Lecturer A",
                    LecturerCode = "LT1"
                },
                SubjectId = 1,
                Subject = new Subject()
                {
                    SubjectName = "subject 1",
                    SubjectCode = "SB1"
                },
                Status = 1,
                Objectives = new List<Objective>()
                {
                    new Objective()
                    {
                        Description = "Objective A",
                        Priority = "high",
                        ObjectiveMilestones = new List<ObjectiveMilestone>()
                        {
                            new ObjectiveMilestone()
                            {
                                Title = "Milestone 1",
                                Description = "Milestone Description 1",
                                StartDate = new DateOnly(year: 2012, month: 12, day: 25),
                                EndDate = new DateOnly(year: 2013, month: 1, day: 12),
                            }
                        }
                    }
                },
            };

            // Act
            var projectVM = (ProjectVM)project;

            // Assert
            Assert.Equal(1, projectVM.ProjectId);
            Assert.Equal("Supplies Exchange Web App", projectVM.ProjectName);
            Assert.Equal("A web app for student to exchange school supplies", projectVM.Description);
            Assert.Equal(1, projectVM.LecturerId);
            Assert.Equal("Lecturer A", projectVM.LecturerName);
            Assert.Equal("LT1", projectVM.LecturerCode);
            Assert.Equal(1, projectVM.SubjectId);
            Assert.Equal("subject 1", projectVM.SubjectName);
            Assert.Equal("SB1", projectVM.SubjectCode);
            Assert.Equal(1, projectVM.Status);

            var objective = projectVM.Objectives[0];
            Assert.Equal("Objective A", objective.Description);
            Assert.Equal("high", objective.Priority);

            var milestone = objective.ObjectiveMilestones[0];
            Assert.Equal("Milestone 1", milestone.Title);
            Assert.Equal("Milestone Description 1", milestone.Description);
            Assert.Equal(new DateOnly(year: 2012, month: 12, day: 25), milestone.StartDate);
            Assert.Equal(new DateOnly(year: 2013, month: 1, day: 12), milestone.EndDate);
        }
    }
}
