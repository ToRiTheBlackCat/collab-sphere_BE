using CollabSphere.Application.Constants;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Infrastructure.Base;
using CollabSphere.Infrastructure.PostgreDbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        public async Task<List<User>?> SearchLecturer(string? email, string? fullName, int yob, string? lecturerCode, string? major, int pageNumber, int pageSize, bool isDesc)
        {
            var queryList = _context.Users
                .Include(x => x.Role)
                .Include(x => x.Lecturer)
                .Where(x => x.RoleId == RoleConstants.LECTURER)
                .AsQueryable();

            //Search
            if (!string.IsNullOrEmpty(email))
                queryList = queryList.Where(x => x.Email.Contains(email));

            if (!string.IsNullOrEmpty(fullName))
                queryList = queryList.Where(x => x.Lecturer.Fullname.Contains(fullName));

            if (yob > 0)
                queryList = queryList.Where(x => x.Lecturer.Yob == yob);

            if (!string.IsNullOrEmpty(lecturerCode))
                queryList = queryList.Where(x => x.Lecturer.LecturerCode.Contains(lecturerCode));

            if (!string.IsNullOrEmpty(major))
                queryList = queryList.Where(x => x.Lecturer.Major.Contains(major));

            //Order by name
            queryList = isDesc
                ? queryList.OrderByDescending(x => x.UId)
                : queryList.OrderBy(x => x.UId);

            // Paging
            var totalItems = await queryList.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagingResult = await queryList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return pagingResult;
        }
    }
}
