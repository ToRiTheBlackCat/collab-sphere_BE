using CollabSphere.Application.Constants;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Infrastructure.Repositories
{

    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(collab_sphereContext context) : base(context)
        {

        }
        public async Task<User?> GetStudentById(int studentId)
        {
            return await _context.Users
                .Include(x => x.Student)
                .FirstOrDefaultAsync(x => x.UId == studentId && x.IsActive);
        }

        public async Task InsertStudent(Student student)
        {
            await _context.Students.AddAsync(student);
        }

        public void UpdateStudent(Student student)
        {
            _context.Students.Update(student);
        }

        public async Task<List<User>?> SearchStudent(string? email, string? fullName, int yob, string? studentCode, string? major, bool isDesc)
        {
            var queryList = _context.Users
               .Include(x => x.Role)
               .Include(x => x.Student)
               .Where(x => x.RoleId == RoleConstants.STUDENT)
               .AsQueryable();

            //Search
            if (!string.IsNullOrEmpty(email))
                queryList = queryList.Where(x => x.Email.Contains(email));

            if (!string.IsNullOrEmpty(fullName))
                queryList = queryList.Where(x => x.Student.Fullname.Contains(fullName));

            if (yob > 0)
                queryList = queryList.Where(x => x.Student.Yob == yob);

            if (!string.IsNullOrEmpty(studentCode))
                queryList = queryList.Where(x => x.Student.StudentCode.Contains(studentCode));

            if (!string.IsNullOrEmpty(major))
                queryList = queryList.Where(x => x.Student.Major.Contains(major));

            //Order by name
            queryList = isDesc
                ? queryList.OrderByDescending(x => x.UId)
                : queryList.OrderBy(x => x.UId);

            return await queryList.ToListAsync();
        }
    }
}
