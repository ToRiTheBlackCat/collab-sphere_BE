namespace CollabSphere.Application.DTOs.Teams
{
    public class AllTeamByAssignClassDto
    {
        public int TeamId { get; set; }

        public string TeamName { get; set; } = string.Empty;

        public string TeamImage { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int? ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;

        public DateOnly CreatedDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public float? Progress { get; set; } = 0;

        public int Status { get; set; }
    }
}
