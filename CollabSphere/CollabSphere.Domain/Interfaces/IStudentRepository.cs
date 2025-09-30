using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Intefaces
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
        Task InsertStudent(Student student);
        void UpdateStudent(Student student);
    }
}
