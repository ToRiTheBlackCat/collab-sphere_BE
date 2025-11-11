using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.TeamWorkspace
{
    public class TeamWorkspaceDetailDto
    {
        public int WorkspaceId { get; set; }
        public int TeamId { get; set; }
        public string? Title { get; set; }
        public DateOnly CreatedAt { get; set; }

        //List 
        public List<ListDto> ListDtos { get; set; } = new List<ListDto>();
    }

    #region ListDto
    public class ListDto
    {
        public int ListId { get; set; }
        public float Position { get; set; }
        public string? Title { get; set; }
        public List<CardDto> CardDtos { get; set; } = new List<CardDto>();
    }
    #endregion

    #region Card
    public class CardDto
    {
        public int CardId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? RiskLevel { get; set; }
        public float Position { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueAt { get; set; }
        public bool? IsComplete { get; set; }
        public List<CardAssignmentDto> CardAssignmentDtos { get; set; } = new List<CardAssignmentDto>();
        public List<TaskDto> TaskDtos { get; set; } = new List<TaskDto>();
    }
    #endregion

    #region CardAssignment
    public class CardAssignmentDto
    {
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? AvatarImg { get; set; }
    }
    #endregion

    #region Task
    public class TaskDto
    {
        public int TaskId { get; set; }
        public string? TaskTitle { get; set; }
        public float Order { get; set; }
        public List<SubTaskDto> SubTaskDtos { get; set; } = new List<SubTaskDto>();

    }
    #endregion

    #region SubTask
    public class SubTaskDto
    {
        public int SubTaskId { get; set; }
        public string? SubTaskTitle { get; set; }
        public float Order { get; set; }
        public bool? IsDone { get; set; }
    }
    #endregion
}
