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
    public class StudentRepository : IStudentRepository
    {
        private readonly collab_sphereContext _context;
        public StudentRepository(collab_sphereContext context)
        {
            _context = context;
        }

        public async Task InsertStudent(Student student)
        {
            await _context.Students.AddAsync(student);
        }

        public void UpdateStudent(Student student)
        {
            _context.Students.Update(student);
        }
    }
}
