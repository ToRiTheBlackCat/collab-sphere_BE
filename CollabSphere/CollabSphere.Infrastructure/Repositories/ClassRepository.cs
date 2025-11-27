using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace CollabSphere.Infrastructure.Repositories
{
    public class ClassRepository : GenericRepository<Class>, IClassRepository
    {
        public ClassRepository(collab_sphereContext context) : base(context) { }

        public override async Task<List<Class>> GetAll()
        {
            var classes = _context.Classes
                .AsNoTracking()
                .Include(x => x.Subject)
                .Include(x => x.Lecturer)
                .ToListAsync();

            return await classes;
        }

        public override async Task<Class?> GetById(int classId)
        {
            var selectedClass = _context.Classes
                .AsNoTracking()
                .Include(x => x.Subject)
                .Include(x => x.Semester)
                .Include(x => x.Lecturer)
                .Include(x => x.ClassMembers)
                    .ThenInclude(x => x.Student)
                .Include(x => x.ClassMembers)
                    .ThenInclude(x => x.Team)
                .Include(x => x.ClassFiles)
                    .ThenInclude(cFiles => cFiles.User)
                        .ThenInclude(user => user.Lecturer)
                .Include(x => x.ProjectAssignments)
                    .ThenInclude(x => x.Project)
                .Include(x => x.Teams)
                    .ThenInclude(x => x.ProjectAssignment)
                        .ThenInclude(x => x.Project)
                .FirstOrDefaultAsync(x =>
                    x.ClassId == classId
                );

            return await selectedClass;
        }

        public async Task<Class?> GetClassDetail(int classId)
        {
            var selectedClass = _context.Classes
                .AsNoTracking()
                .Include(x => x.Subject)
                .Include(x => x.Semester)
                .Include(x => x.Lecturer)
                .Include(x => x.ClassMembers)
                    .ThenInclude(x => x.Student)
                        .ThenInclude(x => x.StudentNavigation)
                .Include(x => x.ClassMembers)
                    .ThenInclude(x => x.Team)
                .Include(x => x.ClassFiles)
                    .ThenInclude(cFiles => cFiles.User)
                        .ThenInclude(user => user.Lecturer)
                .Include(x => x.ProjectAssignments)
                    .ThenInclude(x => x.Project)
                .Include(x => x.Teams)
                    .ThenInclude(x => x.ProjectAssignment)
                        .ThenInclude(x => x.Project)
                .FirstOrDefaultAsync(x =>
                    x.ClassId == classId
                );

            return await selectedClass;
        }

        public async Task<List<Class>> GetClassByStudentId(int studentId, HashSet<int>? subjectIds = null, string descriptor = "", int? semesterId = null, bool? isActive = null, string orderby = "", bool descending = false)
        {
            var classesQuery = _context.ClassMembers
                .AsNoTracking()
                .Where(x => x.StudentId == studentId)
                .Include(x => x.Class)
                    .ThenInclude(x => x.Subject)
                .Include(x => x.Class)
                    .ThenInclude(x => x.Lecturer)
                .Include(x => x.Class)
                    .ThenInclude(x => x.Semester)
                .Select(x => x.Class)
                .AsQueryable();

            if (subjectIds != null && subjectIds.Any())
            {
                classesQuery = classesQuery.Where(x => subjectIds.Contains(x.SubjectId));
            }

            var classes = FilterClasses(await classesQuery.ToListAsync(), descriptor, semesterId, isActive, orderby, descending);

            return classes;
        }

        public async Task<List<Class>> GetClassByLecturerId(int lecturerId, HashSet<int>? subjectIds = null, string descriptor = "", int? semesterId = null, bool? isActive = null, string orderby = "", bool descending = false)
        {
            var classesQuery = _context.Classes
                .AsNoTracking()
                .Include(x => x.Subject)
                .Include(x => x.Semester)
                .Include(x => x.Lecturer)
                .Where(x =>
                    x.Lecturer.LecturerId == lecturerId
                );

            if (subjectIds != null && subjectIds.Any())
            {
                classesQuery = classesQuery.Where(x => subjectIds.Contains(x.SubjectId));
            }

            var classes = FilterClasses(await classesQuery.ToListAsync(), descriptor, semesterId, isActive, orderby, descending);

            return classes;
        }

        public async Task<List<Class>> SearchClasses(string descriptor = "", int? semesterId = null, HashSet<int>? lecturerIds = null, HashSet<int>? subjectIds = null, bool? isActive = null, string orderby = "", bool descending = false)
        {
            var classesQuery = _context.Classes
                .Include(x => x.Subject)
                .Include(x => x.Semester)
                .Include(x => x.Lecturer)
                .AsNoTracking();

            if (lecturerIds != null && lecturerIds.Any())
            {
                classesQuery = classesQuery.Where(x => lecturerIds.Contains(x.LecturerId!.Value));
            }

            if (subjectIds != null && subjectIds.Any())
            {
                classesQuery = classesQuery.Where(x => subjectIds.Contains(x.SubjectId));
            }

            var classes = FilterClasses(await classesQuery.ToListAsync(), descriptor, semesterId, isActive, orderby, descending);

            return classes;
        }

        private List<Class> FilterClasses(IEnumerable<Class> classes, string descriptor = "", int? semesterId = null, bool? isActive = null, string orderby = "", bool descending = false)
        {
            if (!classes.Any())
            {
                return new List<Class>();
            }

            if (isActive.HasValue)
            {
                classes = classes.Where(x => x.IsActive == isActive);
            }

            if (semesterId.HasValue)
            {
                classes = classes.Where(x => x.SemesterId == semesterId.Value);
            }

            if (!string.IsNullOrWhiteSpace(descriptor))
            {
                // Keywords ranking
                //var nameKeywords = new HashSet<string>(className.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries));
                var descriptorKeywords = new HashSet<string>(descriptor.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries));
                var ranking = classes
                    .Select(cls =>
                    {
                        var fullDescriptor = $"{cls.ClassName} {cls.Subject.SubjectName} {cls.Subject.SubjectCode} {cls.Semester.SemesterName} {cls.Semester.SemesterCode}".ToLower();
                        double weight = 0.0;
                        double wordOrderMultiplier = 1.0; // Later words has less weight

                        foreach (var kw in descriptorKeywords)
                        {
                            if (fullDescriptor.Contains(kw))
                            {
                                // boost for earlier keywords
                                weight += 4 * wordOrderMultiplier;

                                // additional boost if starts with keyword
                                if (fullDescriptor.StartsWith(kw))
                                    weight += 2 * wordOrderMultiplier;
                            }

                            wordOrderMultiplier = Math.Max(0.05, wordOrderMultiplier - 0.05);
                        }

                        // extra boost if exact match of name
                        var name = cls.ClassName.ToLower();
                        if (string.Equals(name, descriptor, StringComparison.OrdinalIgnoreCase))
                            weight += 5;

                        return new
                        {
                            Class = cls,
                            Weight = weight
                        };
                    })
                    .Where(x => x.Weight > 0)
                    .OrderByDescending(x => x.Weight)
                        .ThenBy(x => x.Class.ClassName);

                classes = ranking.Select(x => x.Class).ToList();
            }

            if (!string.IsNullOrWhiteSpace(orderby))
            {
                orderby = orderby.Trim();
                orderby = $"{char.ToUpper(orderby[0])}{orderby.Substring(1)}";

                var orderedList = orderby switch
                {
                    nameof(ClassVM.ClassId) => descending ? classes.OrderByDescending(x => x.ClassId) : classes.OrderBy(x => x.ClassId),
                    nameof(ClassVM.ClassName) => descending ? classes.OrderByDescending(x => x.ClassName) : classes.OrderBy(x => x.ClassName),
                    nameof(ClassVM.LecturerId) => GroupAndOrder(source: classes, keySelector: x => x.LecturerId, subSelector: x => x.ClassName, descending),
                    nameof(ClassVM.LecturerCode) => GroupAndOrder(source: classes, keySelector: x => x.Lecturer.LecturerCode, subSelector: x => x.ClassName, descending),
                    nameof(ClassVM.LecturerName) => GroupAndOrder(source: classes, keySelector: x => x.Lecturer.Fullname, subSelector: x => x.ClassName, descending),
                    nameof(ClassVM.SubjectId) => GroupAndOrder(source: classes, keySelector: x => x.SubjectId, subSelector: x => x.ClassName, descending),
                    nameof(ClassVM.SubjectCode) => GroupAndOrder(source: classes, keySelector: x => x.Subject.SubjectCode, subSelector: x => x.ClassName, descending),
                    nameof(ClassVM.SubjectName) => GroupAndOrder(source: classes, keySelector: x => x.Subject.SubjectName, subSelector: x => x.ClassName, descending),
                    nameof(ClassVM.MemberCount) => GroupAndOrder(source: classes, keySelector: x => x.MemberCount, subSelector: x => x.ClassName, descending),
                    nameof(ClassVM.TeamCount) => GroupAndOrder(source: classes, keySelector: x => x.TeamCount, subSelector: x => x.ClassName, descending),
                    nameof(ClassVM.CreatedDate) => GroupAndOrder(source: classes, keySelector: x => x.CreatedDate, subSelector: x => x.ClassName, descending),
                    nameof(ClassVM.IsActive) => GroupAndOrder(source: classes, keySelector: x => x.IsActive, subSelector: x => x.ClassName, descending),
                    nameof(ClassVM.SemesterId) => GroupAndOrder(source: classes, keySelector: x => x.SemesterId, subSelector: x => x.ClassName, descending),
                    nameof(ClassVM.SemesterName) => GroupAndOrder(source: classes, keySelector: x => x.Semester.SemesterName, subSelector: x => x.ClassName, descending),
                    _ => classes.OrderBy(x => 0),
                };

                classes = orderedList;
            }

            return classes.ToList();
        }

        public static IEnumerable<T> GroupAndOrder<T, TKey, TSubKey>(
            IEnumerable<T> source,
            Func<T, TKey> keySelector,
            Func<T, TSubKey> subSelector,
            bool descending = false)
        {
            var grouped = descending
                ? source.GroupBy(keySelector).OrderByDescending(g => g.Key)
                : source.GroupBy(keySelector).OrderBy(g => g.Key);

            return grouped.SelectMany(g => g.OrderBy(subSelector));
        }

        public async Task<Class?> GetClassByIdAsync(int classId)
        {
            return await _context.Classes
                .Include(x => x.ClassMembers)
                    .ThenInclude(x => x.Team)
                .Include(x => x.ProjectAssignments)
                    .ThenInclude(x => x.Project)
                .FirstOrDefaultAsync(x => x.ClassId == classId && x.IsActive);
        }

    }
}
