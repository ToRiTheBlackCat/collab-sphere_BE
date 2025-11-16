using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Domain.Intefaces
{
    public interface ILecturerRepository : IGenericRepository<Lecturer>
    {
        Task InsertLecturer(Lecturer lecturer);
        void UpdateLecturer(Lecturer lecturer);

        Task<List<User>?> SearchLecturer(string? email, string? fullName, int yob, string? lecturerCode, string? major, bool isDesc);
    }
}
