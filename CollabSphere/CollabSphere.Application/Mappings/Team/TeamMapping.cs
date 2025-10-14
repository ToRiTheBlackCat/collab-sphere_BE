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

        public static StudentTeamByAssignClassDto? Team_To_StudentTeamByAssignClassDto(this Domain.Entities.Team team)
        {
            var dto = new StudentTeamByAssignClassDto
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

            return dto;
        }
    }
}
