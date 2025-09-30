﻿using CollabSphere.Domain.Entities;
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

     public class StudentRepository: GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(collab_sphereContext context) : base(context) 
        {

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
