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
        public static List<AllTeamOfStudentDto> ListTeam_To_AllTeamOfStudentDto(this List<Domain.Entities.Team>? list)
        {
            if (list.Any() == false || list == null)
            {
                return new List<AllTeamOfStudentDto>();
            }


            var dtoList = new List<AllTeamOfStudentDto>();
            foreach (var team in list)
            {
                if (team == null)
                {
                    continue;
                }

                var dto = new AllTeamOfStudentDto
                {
                    TeamId = team.TeamId,
                    TeamName = team.TeamName,
                    TeamImage = team.TeamImage,
                    ClassId = team.ClassId,
                    ClassName = team.Class?.ClassName ?? "",
                    LecturerId = team.LecturerId,
                    LecturerName = team.LecturerName,
                    ProjectId = team.ProjectAssignment.ProjectId,
                    ProjectName = team.ProjectAssignment.Project.ProjectName ?? "",
                    Progress = team.Progress
                };
                dtoList.Add(dto);
            }

            return dtoList;
        }
    }
}
