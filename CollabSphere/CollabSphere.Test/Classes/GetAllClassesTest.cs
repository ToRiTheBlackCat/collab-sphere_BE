using CollabSphere.Application;
using CollabSphere.Application.Features.Classes.Queries.GetAllClasses;
using CollabSphere.Domain.Entities;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
using CollabSphere.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Classes
{
    public class GetAllClassesTest
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

        public GetAllClassesTest()
        {
            
        }

        [Fact]
        public async Task Handle_ShouldGetClassesByTeacherIds()
        {
            // Arrange
            var context = GetInMemoryContext("LecturerIdsTest");

            var semester = new Semester { SemesterId = 1, SemesterName = "Fall 2025", SemesterCode = "FA25", StartDate = new DateOnly(2025, 10, 1), EndDate = new DateOnly(2025, 12, 1) };

            var lecturer1 = new Lecturer { LecturerId = 1, Fullname = "Dr. Smith", LecturerCode = "L001" };
            var lecturer2 = new Lecturer { LecturerId = 2, Fullname = "Dr. Adams", LecturerCode = "L002" };
            var lecturer3 = new Lecturer { LecturerId = 3, Fullname = "Dr. Robins", LecturerCode = "L003" };

            var subject1 = new Subject { SubjectId = 1, SubjectName = "Math", SubjectCode = "MATH101" };
            var subject2 = new Subject { SubjectId = 2, SubjectName = "Literature", SubjectCode = "LIT101" };

            context.Classes.Add(new Class { ClassId = 1, ClassName = "Algebra", EnrolKey = "12345", Lecturer = lecturer1, LecturerId = 1, Subject = subject1, SemesterId = 1, Semester = semester });
            context.Classes.Add(new Class { ClassId = 2, ClassName = "Geometry", EnrolKey = "12345", Lecturer = lecturer2, LecturerId = 2, Subject = subject1, SemesterId = 1, Semester = semester });
            context.Classes.Add(new Class { ClassId = 3, ClassName = "Critic Writing", EnrolKey = "12345", Lecturer = lecturer3, LecturerId = 3, Subject = subject2, SemesterId = 1, Semester = semester });
            context.SaveChanges();

            var unitOfWork = new UnitOfWork(context);

            var query = new GetAllClassesQuery()
            {
                LecturerIds = new HashSet<int> { 1, 2 }
            };

            var handler = new GetAllClassesHandler(unitOfWork);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotEmpty(result.PaginatedClasses!.List);
            Assert.Equal(2, result.PaginatedClasses!.ItemCount);
            Assert.DoesNotContain("Critic Writing", result.PaginatedClasses.List.Select(x => x.ClassName));
        }

        [Fact]
        public async Task Handle_ShouldGetClassesBySubjectIds()
        {
            // Arrange
            var context = GetInMemoryContext("SubjectIdsTest");
            var semester = new Semester { SemesterId = 1, SemesterName = "Fall 2025", SemesterCode = "FA25", StartDate = new DateOnly(2025, 10, 1), EndDate = new DateOnly(2025, 12, 1) };

            var lecturer1 = new Lecturer { LecturerId = 1, Fullname = "Dr. Smith", LecturerCode = "L001" };
            var lecturer2 = new Lecturer { LecturerId = 2, Fullname = "Dr. Adams", LecturerCode = "L002" };
            var lecturer3 = new Lecturer { LecturerId = 3, Fullname = "Dr. Robins", LecturerCode = "L003" };

            var subject1 = new Subject { SubjectId = 1, SubjectName = "Math", SubjectCode = "MATH101" };
            var subject2 = new Subject { SubjectId = 2, SubjectName = "Literature", SubjectCode = "LIT101" };

            context.Classes.Add(new Class { ClassId = 1, ClassName = "Algebra", EnrolKey = "12345", Lecturer = lecturer1, LecturerId = 1, Subject = subject1, SemesterId = 1, Semester = semester });
            context.Classes.Add(new Class { ClassId = 2, ClassName = "Geometry", EnrolKey = "12345", Lecturer = lecturer2, LecturerId = 2, Subject = subject1, SemesterId = 1, Semester = semester });
            context.Classes.Add(new Class { ClassId = 3, ClassName = "Critic Writing", EnrolKey = "12345", Lecturer = lecturer3, LecturerId = 3, Subject = subject2, SemesterId = 1, Semester = semester });
            context.SaveChanges();

            var unitOfWork = new UnitOfWork(context);

            var query = new GetAllClassesQuery()
            {
                SubjectIds = new HashSet<int> { 1 }
            };

            var handler = new GetAllClassesHandler(unitOfWork);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.PaginatedClasses);
            Assert.NotEmpty(result.PaginatedClasses.List);
            Assert.Equal(2, result.PaginatedClasses.ItemCount);
            Assert.DoesNotContain(result.PaginatedClasses.List, x => x.SubjectId != 1);
        }

        [Fact]
        public async Task Handle_ShouldPaginateClasses()
        {
            // Arrange
            var context = GetInMemoryContext("PagingTest");

            var semester = new Semester { SemesterId = 1, SemesterName = "Fall 2025", SemesterCode = "FA25", StartDate = new DateOnly(2025, 10, 1), EndDate = new DateOnly(2025, 12, 1) };

            var lecturer1 = new Lecturer { LecturerId = 1, Fullname = "Dr. Smith", LecturerCode = "L001" };
            var lecturer2 = new Lecturer { LecturerId = 2, Fullname = "Dr. Adams", LecturerCode = "L002" };
            var lecturer3 = new Lecturer { LecturerId = 3, Fullname = "Dr. Robins", LecturerCode = "L003" };

            var subject1 = new Subject { SubjectId = 1, SubjectName = "Math", SubjectCode = "MATH101" };
            var subject2 = new Subject { SubjectId = 2, SubjectName = "Literature", SubjectCode = "LIT101" };

            context.Classes.Add(new Class { ClassId = 1, ClassName = "Algebra", EnrolKey = "12345", Lecturer = lecturer1, LecturerId = 1, Subject = subject1, SemesterId = 1, Semester = semester });
            context.Classes.Add(new Class { ClassId = 2, ClassName = "Geometry", EnrolKey = "12345", Lecturer = lecturer2, LecturerId = 2, Subject = subject1, SemesterId = 1, Semester = semester });
            context.Classes.Add(new Class { ClassId = 3, ClassName = "Critic Writing", EnrolKey = "12345", Lecturer = lecturer3, LecturerId = 3, Subject = subject2, SemesterId = 1, Semester = semester });
            context.SaveChanges();

            var unitOfWork = new UnitOfWork(context);

            var query = new GetAllClassesQuery()
            {
                PageSize = 1,
                PageNum = 2,
                OrderBy = ""
            };

            var handler = new GetAllClassesHandler(unitOfWork);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);
            var test = await unitOfWork.ClassRepo.GetAll();

            // Assert
            Assert.NotNull(result.PaginatedClasses);
            Assert.NotEmpty(result.PaginatedClasses.List);
            Assert.Equal(3, result.PaginatedClasses.ItemCount);
            Assert.Equal(3, result.PaginatedClasses.PageCount);
            Assert.Equal("Geometry", result.PaginatedClasses.List.First().ClassName);
        }
    }
}
