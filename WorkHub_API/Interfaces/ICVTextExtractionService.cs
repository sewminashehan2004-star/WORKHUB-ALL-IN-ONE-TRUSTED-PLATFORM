namespace WorkHub.API.Interfaces
{
    public interface ICVTextExtractionService
    {
        Task<string> ExtractTextAsync(string physicalFilePath);
    }
}