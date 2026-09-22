namespace WorkHub.API.Models
{
    public class CVDocument
    {
        public int CVDocumentId { get; set; }

        public int JobSeekerProfileId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string StoredFileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public bool IsPrimary { get; set; } = true;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public JobSeekerProfile JobSeekerProfile { get; set; } = null!;
    }
}