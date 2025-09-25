using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Infrastructure.PostgreDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Infrastructure.Repositories
{
    public class LecturerRepository : ILecturerRepository
    {
        private readonly collab_sphereContext _context;
        public LecturerRepository(collab_sphereContext context)
        {
            _context = context;
        }

        public async Task InsertLecturer(Lecturer lecturer)
        {
            await _context.Lecturers.AddAsync(lecturer);
        }

        public void UpdateLecturer(Lecturer lecturer)
        {
            _context.Lecturers.Update(lecturer);
        }
    }
}
