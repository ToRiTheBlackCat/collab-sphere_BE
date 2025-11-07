using CollabSphere.Application.Base;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CollabSphere.Application.Features.Evaluate.Commands.StudentEvaluateOtherInTeam
{
    public class StudentEvaluateOtherInTeamCommand : ICommand
    {
        [JsonIgnore]
        public int TeamId { get; set; }
        [JsonIgnore]
        public int RaterId = -1;
        [JsonIgnore]
        public int RaterRole = -1;

        public List<EvaluatorDetail> EvaluatorDetails { get; set; } = new List<EvaluatorDetail>();
    }

    public class EvaluatorDetail
    {
        [Required]
        public int ReceiverId { get; set; }
        public List<ScoreDetail> ScoreDetails { get; set; } = new List<ScoreDetail>();
    }

    public class ScoreDetail
    {
        [Required]
        public string ScoreDetailName { get; set; } = string.Empty;

        [Required]
        [Range(0, 5)]
        public int Score { get; set; } = 0;

    }
}
