using LearningManagementSystem.Application.DTOs.Certificates;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LearningManagementSystem.Web.Services;

/// <summary>
/// Renders a downloadable certificate PDF from certificate data using QuestPDF (Community license).
/// </summary>
public static class CertificatePdfService
{
    public static byte[] Generate(CertificateDto certificate)
    {
        var studentDisplay = string.IsNullOrWhiteSpace(certificate.StudentName) ? certificate.StudentId : certificate.StudentName;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontFamily("Arial"));

                page.Content()
                    .Border(3)
                    .BorderColor("#5624D0")
                    .Padding(40)
                    .Column(column =>
                    {
                        column.Spacing(18);

                        column.Item().AlignCenter().Text("AI SKILLS ACADEMY")
                            .FontSize(24).Bold().FontColor("#5624D0");

                        column.Item().AlignCenter().Text("CERTIFICATE OF COMPLETION")
                            .FontSize(14).FontColor("#6a6f73");

                        column.Item().PaddingTop(20).AlignCenter().Text("This is to certify that")
                            .FontSize(13);

                        column.Item().AlignCenter().Text(studentDisplay)
                            .FontSize(32).Bold().FontColor("#1c1d1f");

                        column.Item().AlignCenter().Text("has successfully completed the course")
                            .FontSize(13);

                        column.Item().AlignCenter().Text(certificate.CourseTitle)
                            .FontSize(20).Bold().FontColor("#5624D0");

                        column.Item().PaddingTop(30).Row(row =>
                        {
                            row.RelativeItem().AlignCenter().Column(col =>
                            {
                                col.Item().AlignCenter().Text("Issued Date").FontSize(10).FontColor("#6a6f73");
                                col.Item().AlignCenter().Text(certificate.IssuedDate.ToString("MMMM dd, yyyy")).FontSize(13).Bold();
                            });
                            row.RelativeItem().AlignCenter().Column(col =>
                            {
                                col.Item().AlignCenter().Text("Certificate Number").FontSize(10).FontColor("#6a6f73");
                                col.Item().AlignCenter().Text(certificate.CertificateNumber).FontSize(13).Bold();
                            });
                        });

                        column.Item().PaddingTop(20).AlignCenter().Text("Verified Authentic Credential")
                            .FontSize(10).FontColor("#28a745");
                    });
            });
        });

        return document.GeneratePdf();
    }
}
