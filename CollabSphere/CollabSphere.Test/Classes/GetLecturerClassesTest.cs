using CollabSphere.Application.Features.Classes.Queries.GetLecturerClasses;
using CollabSphere.Domain.Entities;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Classes
{
    public class GetLecturerClassesTest
    {
        private collab_sphereContext GetInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<collab_sphereContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new collab_sphereContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private collab_sphereContext SetUpContext(collab_sphereContext context)
        {
            var lecturer1 = new Lecturer { LecturerId = 1, Fullname = "Dr. Smith", LecturerCode = "L001" };
            var lecturer2 = new Lecturer { LecturerId = 2, Fullname = "Dr. Adams", LecturerCode = "L002" };
            //var lecturer3 = new Lecturer { LecturerId = 3, Fullname = "Dr. Robins", LecturerCode = "L003" };

            var subject1 = new Subject { SubjectId = 1, SubjectName = "Math", SubjectCode = "MATH101" };
            var subject2 = new Subject { SubjectId = 2, SubjectName = "Literature", SubjectCode = "LIT101" };

            context.Classes.Add(new Class { ClassId = 1, ClassName = "Algebra", EnrolKey = "12345", Lecturer = lecturer1, LecturerId = 1, Subject = subject1 });
            context.Classes.Add(new Class { ClassId = 2, ClassName = "Geometry", EnrolKey = "12345", Lecturer = lecturer2, LecturerId = 2, Subject = subject1 });
            context.Classes.Add(new Class { ClassId = 3, ClassName = "Critic Writing", EnrolKey = "12345", Lecturer = lecturer1, LecturerId = 1, Subject = subject2 });
            context.SaveChanges();

            return context;
        }

        public GetLecturerClassesTest()
        {
        }

        [Fact]
        public async Task Handle_ShouldReturnClassesOfLecturer()
        {
            // Arrange
            var unitOfWork = new UnitOfWork(SetUpContext(GetInMemoryContext("LecturerIdTest")));

            var query = new GetLecturerClassesQuery()
            {
                LecturerId = 1
            };

            var handler = new GetLecturerClassesHandler(unitOfWork);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.PaginatedClasses);
            Assert.NotEmpty(result.PaginatedClasses.List);
            Assert.Equal(2, result.PaginatedClasses.ItemCount);
            Assert.DoesNotContain(result.PaginatedClasses.List, x => x.LecturerId != 1);
        }

        [Fact]
        public async Task Handle_ShouldFilterByClassName()
        {
            // Arrange
            var unitOfWork = new UnitOfWork(SetUpContext(GetInMemoryContext("ClassNameTest")));

            var query = new GetLecturerClassesQuery()
            {
                LecturerId = 1,
                ClassName = "critic"
            };

            var handler = new GetLecturerClassesHandler(unitOfWork);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.PaginatedClasses);
            Assert.NotEmpty(result.PaginatedClasses.List);
            Assert.Equal(1, result.PaginatedClasses.ItemCount);
            Assert.Contains("Critic", result.PaginatedClasses.List.First().ClassName);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenNoMatchingClasses()
        {
            // Arrange
            var unitOfWork = new UnitOfWork(SetUpContext(GetInMemoryContext("EmptyTest")));

            var query = new GetLecturerClassesQuery()
            {
                LecturerId = 2,
                ClassName = "critic"
            };

            var handler = new GetLecturerClassesHandler(unitOfWork);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.PaginatedClasses);
            Assert.Empty(result.PaginatedClasses.List);
            Assert.Equal(0, result.PaginatedClasses.ItemCount);
        }
    }
}
