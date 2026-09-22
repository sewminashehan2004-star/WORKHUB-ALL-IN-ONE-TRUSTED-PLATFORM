using DocumentFormat.OpenXml.Packaging;
using System.Text;
using UglyToad.PdfPig;
using WorkHub.API.Interfaces;

namespace WorkHub.API.Services
{
    public class CVTextExtractionService
        : ICVTextExtractionService
    {
        public Task<string> ExtractTextAsync(
            string physicalFilePath)
        {
            if (string.IsNullOrWhiteSpace(
                physicalFilePath))
            {
                return Task.FromResult(
                    string.Empty);
            }

            if (!File.Exists(
                physicalFilePath))
            {
                return Task.FromResult(
                    string.Empty);
            }

            var extension =
                Path.GetExtension(
                    physicalFilePath)
                .ToLowerInvariant();

            try
            {
                return extension switch
                {
                    ".pdf" =>
                        Task.FromResult(
                            ExtractPdfText(
                                physicalFilePath)),

                    ".docx" =>
                        Task.FromResult(
                            ExtractDocxText(
                                physicalFilePath)),

                    _ =>
                        Task.FromResult(
                            string.Empty)
                };
            }
            catch
            {
                return Task.FromResult(
                    string.Empty);
            }
        }

        private static string ExtractPdfText(
            string path)
        {
            var builder =
                new StringBuilder();

            using var document =
                PdfDocument.Open(path);

            foreach (var page in
                document.GetPages())
            {
                builder.AppendLine(
                    page.Text);
            }

            return builder.ToString();
        }

        private static string ExtractDocxText(
            string path)
        {
            using var document =
                WordprocessingDocument.Open(
                    path,
                    false);

            var body =
                document.MainDocumentPart?
                    .Document
                    .Body;

            if (body == null)
            {
                return string.Empty;
            }

            return body.InnerText;
        }
    }
}