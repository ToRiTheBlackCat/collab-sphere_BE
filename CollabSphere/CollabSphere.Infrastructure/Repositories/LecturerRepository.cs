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
     public class LecturerRepository : GenericRepository<Lecturer>, ILecturerRepository
    {
        public LecturerRepository(collab_sphereContext context) : base(context) 
        {
        
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
