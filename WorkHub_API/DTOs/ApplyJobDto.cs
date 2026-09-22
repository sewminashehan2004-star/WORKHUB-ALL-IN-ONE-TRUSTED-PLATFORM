using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class ApplyJobDto
    {
        public int? CVDocumentId { get; set; }

        [MaxLength(2000)]
        public string? CoverLetter { get; set; }

        public List<ApplyJobScreeningAnswerDto> ScreeningAnswers { get; set; } = new();
    }

    public class ApplyJobScreeningAnswerDto
    {
        public int QuestionId { get; set; }

        [MaxLength(2000)]
        public string? Answer { get; set; }
    }
}
