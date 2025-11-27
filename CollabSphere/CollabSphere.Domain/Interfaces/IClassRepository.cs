using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Intefaces
{
    public interface IClassRepository : IGenericRepository<Class>
    {
        Task<List<Class>> GetClassByLecturerId(int lecturerId, HashSet<int>? subjectIds = null, string descriptor = "", int? semesterId = null, bool? isActive = null, string orderby = "", bool descending = false);
        Task<List<Class>> GetClassByStudentId(int studentId, HashSet<int>? subjectIds = null, string descriptor = "", int? semesterId = null, bool? isActive = null, string orderby = "", bool descending = false);
        Task<List<Class>> SearchClasses(string descriptor = "", int? semesterId = null, HashSet<int>? lecturerIds = null, HashSet<int>? subjectIds = null, bool? isActive = null, string orderby = "", bool descending = false);
        Task<Class?> GetClassDetail(int classId);
        Task<Class?> GetClassByIdAsync(int classId);
    }
}
