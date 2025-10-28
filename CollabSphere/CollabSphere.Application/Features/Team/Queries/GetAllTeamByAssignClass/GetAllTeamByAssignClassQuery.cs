using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CollabSphere.Application.Features.Team.Queries.GetAllTeamByAssignClass
{
    public class GetAllTeamByAssignClassQuery : PaginationQuery, IQuery<GetAllTeamByAssignClassResult>, IValidatableObject
    {
        [FromRoute(Name = "classId")]
        public int ClassId { get; set; }

        [JsonIgnore]
        public int UserId = -1;

        [JsonIgnore]
        public int UserRole = -1;

        [FromQuery]
        public string? TeamName { get; set; }

        [FromQuery]
        public int? ProjectId { get; set; } = 0;

        [FromQuery]
        public DateOnly? FromDate { get; set; }

        [FromQuery]
        public DateOnly? EndDate { get; set; }

        [FromQuery]
        public bool IsDesc { get; set; } = false;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate.HasValue && FromDate.HasValue && EndDate < FromDate)
            {
                yield return new ValidationResult(
                    "EndDate must be after to CreatedDate.",
                    new[] { nameof(EndDate) }
                );
            }
        }
    }
}
