using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Intefaces
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        Task<List<Team>?> GetListTeamOfStudent(int studentId, string? teamName, int? classId, int? semesterId);
        Task<List<Team>?> SearchTeam(int classId, string? teamName, int? projectId, DateOnly? fromDate, DateOnly? endDate, bool isDesc);
        Task<Team?> GetTeamDetail(int teamId);
        Task<Team?> GetTeamWithProjectOverview(int teamId);

    }
}
