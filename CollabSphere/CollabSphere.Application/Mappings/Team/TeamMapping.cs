using CollabSphere.Application.DTOs.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Mappings.Team
{
    public static class TeamMapping
    {
        public static List<AllTeamByAssignClassDto> ListTeam_To_ListTeamByAssignClassDto(this List<Domain.Entities.Team>? list)
        {
            if (list.Any() == false || list == null)
            {
                return new List<AllTeamByAssignClassDto>();
            }


            var dtoList = new List<AllTeamByAssignClassDto>();
            foreach (var team in list)
            {
                if (team == null)
                {
                    continue;
                }

                var dto = new AllTeamByAssignClassDto
                {
                    TeamId = team.TeamId,
                    TeamName = team.TeamName,
                    TeamImage = team.TeamImage,
                    Description = team.Description,
                    ProjectId = team.ProjectAssignment?.ProjectId,
                    ProjectName = team.ProjectAssignment?.Project?.ProjectName ?? "",
                    CreatedDate = team.CreatedDate,
                    EndDate = team.EndDate,
                    Progress = team.Progress,
                    Status = team.Status
                };
                dtoList.Add(dto);
            }

            return dtoList;
        }
    }
}
