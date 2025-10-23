using CollabSphere.Domain.Entities;
using CollabSphere.Infrastructure.PostgreDbContext;
using CollabSphere.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Classes
{
    public class ClassFilterTest
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

        [Fact]
        public void FilterClasses_ShouldOrderByKeywordWeight()
        {
            // Arrange
            var repo = new ClassRepository(GetInMemoryContext("FilterTest"));
            var classes = new List<Class>
            {
                new Class { ClassId = 1, ClassName = "Web Development" },
                new Class { ClassId = 2, ClassName = "Mobile Apps" },
                new Class { ClassId = 3, ClassName = "Web Design" }
            };

            // Act
            var result = repo.FilterClasses(classes, "Web");

            // Assert
            Assert.Equal(2, result.Count); // Only 2 classes contain "Web"
            Assert.Equal("Web Development", result.First().ClassName); // Highest weight should be first
        }

        [Fact]
        public async Task GetClassByStudentId_ShouldReturnClassesForStudent()
        {
            // Arrange
            var context = GetInMemoryContext("StudentTest");

            var semester = new Semester { SemesterId = 1, SemesterName = "Fall 2025", SemesterCode = "FA25", StartDate = new DateOnly(2025, 10, 1), EndDate = new DateOnly(2025, 12, 1) };
            var subject = new Subject { SubjectId = 1, SubjectName = "Math", SubjectCode = "MATH101" };
            var lecturer = new Lecturer { LecturerId = 1, Fullname = "Dr. Smith", LecturerCode = "L001" };

            var cls = new Class { ClassId = 1, ClassName = "Algebra", EnrolKey = "12345", Subject = subject, Lecturer = lecturer, SubjectId = 1, LecturerId = 1, SemesterId = 1, Semester = semester };
            context.Classes.Add(cls);
            context.ClassMembers.Add(new ClassMember { ClassId = 1, StudentId = 42, Fullname = "Jack Roberts" });
            context.SaveChanges();

            var repo = new ClassRepository(context);

            // Act
            var result = await repo.GetClassByStudentId(42);

            // Assert
            Assert.Single(result);
            Assert.Equal("Algebra", result.First().ClassName);
        }

        [Fact]
        public async Task GetClassByLecturerId_ShouldReturnOnlyLecturersClasses()
        {
            // Arrange
            var context = GetInMemoryContext("LecturerFilterTest");

            var semester = new Semester { SemesterId = 1, SemesterName = "Fall 2025", SemesterCode = "FA25", StartDate = new DateOnly(2025, 10, 1), EndDate = new DateOnly(2025, 12, 1) };

            var lecturer1 = new Lecturer { LecturerId = 1, Fullname = "Dr. Smith", LecturerCode = "L001" };
            var lecturer2 = new Lecturer { LecturerId = 2, Fullname = "Dr. Adams", LecturerCode = "L002" };

            var subject = new Subject { SubjectId = 1, SubjectName = "Math", SubjectCode = "MATH101" };

            context.Classes.Add(new Class { ClassId = 1, ClassName = "Algebra", EnrolKey = "12345", Lecturer = lecturer1, LecturerId = 1, Subject = subject, SemesterId = 1, Semester = semester });
            context.Classes.Add(new Class { ClassId = 2, ClassName = "Geometry", EnrolKey = "12345", Lecturer = lecturer2, LecturerId = 2, Subject = subject, SemesterId = 1, Semester = semester });
            context.SaveChanges();

            var repo = new ClassRepository(context);

            // Act
            var result = await repo.GetClassByLecturerId(1);

            // Assert
            Assert.Single(result);
            Assert.Equal("Algebra", result.First().ClassName);
        }

        [Fact]
        public void FilterClasses_ShouldOrderByClassNameDescending()
        {
            var repo = new ClassRepository(GetInMemoryContext("OrderTest"));
            var classes = new List<Class>
            {
                new Class { ClassId = 1, ClassName = "Alpha" },
                new Class { ClassId = 2, ClassName = "Charlie" },
                new Class { ClassId = 3, ClassName = "Bravo" }
            };

            var result = repo.FilterClasses(classes, orderby: "ClassName", descending: true);

            Assert.Equal(new[] { "Charlie", "Bravo", "Alpha" }, result.Select(c => c.ClassName));
        }
    }
}
