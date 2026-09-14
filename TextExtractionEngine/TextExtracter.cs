using UglyToad.PdfPig;
using System.Text;
using DocumentFormat.OpenXml.Packaging;

namespace CVPilotAPI.TextExtractionEngine
{
    public class TextExtracter : IResumeParserService
    {
        public Task<string> ExtractTextFromFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => Task.FromResult(ExtractTextFromPdf(filePath)),
                ".docx" => Task.FromResult(ExtractTextFromDocx(filePath)),
                ".txt" => File.ReadAllTextAsync(filePath),

                _ => throw new NotSupportedException(
                    $"File format '{extension}' is not supported.")
            };
        }

        private string ExtractTextFromPdf(string filePath)
        {
            using var document = PdfDocument.Open(filePath);

            var text = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                if (!string.IsNullOrWhiteSpace(page.Text))
                {
                    text.AppendLine(page.Text);
                }
            }

            return text.ToString();
        }

        private string ExtractTextFromDocx(string filePath)
        {
            using var wordDocument =
                WordprocessingDocument.Open(filePath, false);

            var body = wordDocument.MainDocumentPart?
                .Document?
                .Body;

            return body?.InnerText ?? string.Empty;
        }
    }
}