using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Drawing;
using System.Drawing.Imaging;
using DocumentFormat.OpenXml.Packaging;
using PdfSharp.Drawing.Layout;

namespace ChatMentor.Backend.Core.Services;

public interface IFileConverterService
{
    Task<IFormFile> ConvertToPdfAsync(IFormFile file);
}

public class FileConverterService : IFileConverterService
{
    public async Task<IFormFile> ConvertToPdfAsync(IFormFile file)
    {
        // Check if file is already PDF
        if (file.ContentType == "application/pdf")
        {
            return file;
        }

        // Create temporary files for conversion
        var originalFilePath = Path.GetTempFileName();
        var pdfFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

        try
        {
            // Save the uploaded file
            using (var fileStream = new FileStream(originalFilePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Convert based on file type
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            switch (extension)
            {
                case ".docx":
                case ".doc":
                    ConvertWordToPdf(originalFilePath, pdfFilePath);
                    break;
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".bmp":
                case ".gif":
                    ConvertImageToPdf(originalFilePath, pdfFilePath);
                    break;
                case ".txt":
                    ConvertTextToPdf(originalFilePath, pdfFilePath);
                    break;
                default:
                    throw new NotSupportedException($"File type {extension} is not supported for PDF conversion");
            }

            // Create a new FormFile from the converted PDF
            var fileBytes = await File.ReadAllBytesAsync(pdfFilePath);
            var memoryStream = new MemoryStream(fileBytes);
            
            return new FormFile(
                memoryStream,
                0,
                fileBytes.Length,
                "file",
                Path.GetFileNameWithoutExtension(file.FileName) + ".pdf")
            {
                Headers = file.Headers,
                ContentType = "application/pdf"
            };
        }
        finally
        {
            // Clean up temporary files
            if (File.Exists(originalFilePath))
                File.Delete(originalFilePath);
                
            if (File.Exists(pdfFilePath))
                File.Delete(pdfFilePath);
        }
    }

    private void ConvertWordToPdf(string inputPath, string outputPath)
    {
        // For Word documents, we'll use OpenXML and PDFsharp
        using (var wordDoc = WordprocessingDocument.Open(inputPath, false))
        {
            // Create new PDF document
            var pdfDocument = new PdfDocument();
            var page = pdfDocument.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var textFormatter = new XTextFormatter(gfx);
            
            // Get Word document text
            var body = wordDoc.MainDocumentPart.Document.Body;
            var text = body.InnerText;
            
            // Add text to PDF
            var font = new XFont("Arial", 12);
            var rect = new XRect(40, 40, page.Width - 80, page.Height - 80);
            textFormatter.DrawString(text, font, XBrushes.Black, rect);
            
            // Save the PDF
            pdfDocument.Save(outputPath);
        }
    }

    private void ConvertImageToPdf(string inputPath, string outputPath)
    {
        using (var image = Image.FromFile(inputPath))
        {
            // Create new PDF document
            var pdfDocument = new PdfDocument();
            var page = pdfDocument.AddPage();
            
            // Set page size to match image with some margin
            page.Width = new XUnit(image.Width + 40);
            page.Height = new XUnit(image.Height + 40);
            
            // Draw image on PDF page
            var gfx = XGraphics.FromPdfPage(page);
            using (var xImage = XImage.FromFile(inputPath))
            {
                gfx.DrawImage(xImage, 20, 20);
            }
            
            // Save the PDF
            pdfDocument.Save(outputPath);
        }
    }

    private void ConvertTextToPdf(string inputPath, string outputPath)
    {
        // Read all text
        var text = File.ReadAllText(inputPath);
        
        // Create new PDF document
        var pdfDocument = new PdfDocument();
        var page = pdfDocument.AddPage();
        var gfx = XGraphics.FromPdfPage(page);
        var textFormatter = new XTextFormatter(gfx);
        
        // Add text to PDF
        var font = new XFont("Courier New", 10);
        var rect = new XRect(40, 40, page.Width - 80, page.Height - 80);
        textFormatter.DrawString(text, font, XBrushes.Black, rect);
        
        // Save the PDF
        pdfDocument.Save(outputPath);
    }
}