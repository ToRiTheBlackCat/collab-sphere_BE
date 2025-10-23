using CollabSphere.Application.Features.Classes.Queries.GetStudentClasses;
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
    public class GetStudentClassesTest
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
            var semester = new Semester { SemesterId = 1, SemesterName = "Fall 2025", SemesterCode = "FA25", StartDate = new DateOnly(2025, 10, 1), EndDate = new DateOnly(2025, 12, 1) };

            var lecturer1 = new Lecturer { LecturerId = 1, Fullname = "Dr. Smith", LecturerCode = "L001" };
            var lecturer2 = new Lecturer { LecturerId = 2, Fullname = "Dr. Adams", LecturerCode = "L002" };

            var subject1 = new Subject { SubjectId = 1, SubjectName = "Math", SubjectCode = "MATH101" };
            var subject2 = new Subject { SubjectId = 2, SubjectName = "Literature", SubjectCode = "LIT101" };

            var class1 = new Class { ClassId = 1, ClassName = "Algebra", EnrolKey = "12345", Lecturer = lecturer1, LecturerId = 1, Subject = subject1, SemesterId = 1, Semester = semester };
            var class2 = new Class { ClassId = 2, ClassName = "Geometry", EnrolKey = "12345", Lecturer = lecturer2, LecturerId = 2, Subject = subject1, SemesterId = 1, Semester = semester };
            var class3 = new Class { ClassId = 3, ClassName = "Critic Writing", EnrolKey = "12345", Lecturer = lecturer1, LecturerId = 1, Subject = subject2, SemesterId = 1, Semester = semester };

            var student1 = new Student { StudentId = 4, Fullname = "Saul Goodman", StudentCode = "SE134566" };
            var student2 = new Student { StudentId = 5, Fullname = "Flash Thundercock", StudentCode = "SE153678" };

            var classMember1 = new ClassMember() { ClassMemberId = 1, Class = class1, Student = student1, Fullname = "Saul Goodman" };
            var classMember2 = new ClassMember() { ClassMemberId = 2, Class = class3, Student = student1, Fullname = "Saul Goodman" };
            var classMember3 = new ClassMember() { ClassMemberId = 3, Class = class2, Student = student2, Fullname = "Flash Thundercock" };

            context.ClassMembers.Add(classMember1);
            context.ClassMembers.Add(classMember2);
            context.ClassMembers.Add(classMember3);
            context.SaveChanges();

            return context;
        }

        [Fact]
        public async Task Handle_ShouldReturnStudentClasses()
        {
            // Arrange
            var unitOfWork = new UnitOfWork(SetUpContext(GetInMemoryContext("NoMatchTest")));

            var query = new GetStudentClassesQuery()
            {
                StudentId = 4,
                OrderBy = "classId"
            };

            var handler = new GetStudentClassesHandler(unitOfWork);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.PaginatedClasses);
            Assert.NotEmpty(result.PaginatedClasses.List);
            Assert.Equal(2, result.PaginatedClasses.ItemCount);
            Assert.Equal(new[] { "Algebra", "Critic Writing" }, result.PaginatedClasses.List.Select(x => x.ClassName));
        }

        [Fact]
        public async Task Handle_ShouldFilterClassName()
        {
            // Arrange
            var unitOfWork = new UnitOfWork(SetUpContext(GetInMemoryContext("ClassTest")));

            var query = new GetStudentClassesQuery()
            {
                StudentId = 4,
                ClassName = "algebr",
                OrderBy = "classId"
            };

            var handler = new GetStudentClassesHandler(unitOfWork);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.PaginatedClasses);
            Assert.NotEmpty(result.PaginatedClasses.List);
            Assert.Equal(1, result.PaginatedClasses.ItemCount);
            Assert.Equal(new[] { "Algebra" }, result.PaginatedClasses.List.Select(x => x.ClassName));
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenNoMatchingClasses()
        {
            // Arrange
            var unitOfWork = new UnitOfWork(SetUpContext(GetInMemoryContext("DefaultTest")));

            var query = new GetStudentClassesQuery()
            {
                StudentId = 6,
                OrderBy = "classId"
            };

            var handler = new GetStudentClassesHandler(unitOfWork);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.PaginatedClasses);
            Assert.Empty(result.PaginatedClasses.List);
            Assert.Equal(0, result.PaginatedClasses.ItemCount);
        }
    }
}
