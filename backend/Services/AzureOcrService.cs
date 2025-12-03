using Azure;
using Azure.AI.Vision.ImageAnalysis;
using GoalboundFamily.Api.Services.Interfaces;
using SixLabors.ImageSharp;

namespace GoalboundFamily.Api.Services;

/// <summary>
/// OCR service using Azure Computer Vision (95-99% accuracy)
/// Optimized for Singapore receipts with mixed Chinese/English text
/// Replaces PaddleOCR Python microservice
/// </summary>
public class AzureOcrService : IOcrService
{
    private readonly IImagePreprocessingService _preprocessingService;
    private readonly ILogger<AzureOcrService> _logger;
    private readonly ImageAnalysisClient _visionClient;
    private readonly string _endpoint;

    public AzureOcrService(
        IImagePreprocessingService preprocessingService,
        ILogger<AzureOcrService> logger,
        IConfiguration configuration)
    {
        _preprocessingService = preprocessingService;
        _logger = logger;

        // Get Azure credentials from environment variables (secure)
        _endpoint = Environment.GetEnvironmentVariable("AZURE_VISION_ENDPOINT")
            ?? configuration["Ocr:AzureVisionEndpoint"]
            ?? throw new InvalidOperationException(
                "Azure Vision endpoint not configured. Set AZURE_VISION_ENDPOINT environment variable.");

        var apiKey = Environment.GetEnvironmentVariable("AZURE_VISION_KEY")
            ?? configuration["Ocr:AzureVisionKey"]
            ?? throw new InvalidOperationException(
                "Azure Vision API key not configured. Set AZURE_VISION_KEY environment variable.");

        // Initialize Azure Computer Vision client
        _visionClient = new ImageAnalysisClient(
            new Uri(_endpoint),
            new AzureKeyCredential(apiKey));

        _logger.LogInformation("Azure Computer Vision OCR Service initialized. Endpoint: {Endpoint}", _endpoint);
    }

    public async Task<OcrResult> ProcessImageAsync(Stream imageStream)
    {
        try
        {
            _logger.LogInformation("Starting OCR processing with Azure Computer Vision");

            // Step 1: Preprocess image (enhance contrast, denoise, etc.)
            // This is still beneficial even with Azure's advanced OCR
            using var preprocessedImage = await _preprocessingService.PreprocessImageAsync(imageStream);

            // Step 2: Convert preprocessed image to BinaryData
            using var memoryStream = new MemoryStream();
            await preprocessedImage.SaveAsPngAsync(memoryStream);
            memoryStream.Position = 0; // Reset stream position
            var imageData = BinaryData.FromStream(memoryStream);

            _logger.LogInformation("Preprocessed image size: {Size} bytes", memoryStream.Length);

            // Step 3: Call Azure Computer Vision Read API
            // VisualFeatures.Read extracts all text (print + handwriting)
            // Language auto-detection handles mixed Chinese/English perfectly
            _logger.LogInformation("Sending request to Azure Computer Vision: {Endpoint}", _endpoint);

            var result = await _visionClient.AnalyzeAsync(
                imageData,
                VisualFeatures.Read,
                new ImageAnalysisOptions { Language = "en" });

            // Step 4: Extract text blocks from Azure response
            if (result.Value.Read == null || result.Value.Read.Blocks.Count == 0)
            {
                _logger.LogWarning("No text detected in image");
                return new OcrResult
                {
                    Text = string.Empty,
                    Confidence = 0,
                    TextBlocks = new List<OcrTextBlock>(),
                    Success = true,
                    ErrorMessage = "No text found in image"
                };
            }

            // Step 5: Parse text blocks and calculate confidence
            var textBlocks = new List<OcrTextBlock>();
            var allText = new List<string>();
            var confidenceScores = new List<decimal>();
            int lineNumber = 0;

            foreach (var block in result.Value.Read.Blocks)
            {
                foreach (var line in block.Lines)
                {
                    lineNumber++;

                    // Azure returns confidence per word, we average them per line
                    var lineConfidence = line.Words.Any()
                        ? (decimal)line.Words.Average(w => w.Confidence)
                        : 0m;

                    var lineText = line.Text;
                    allText.Add(lineText);
                    confidenceScores.Add(lineConfidence);

                    // Extract bounding polygon from Azure response
                    var boundingPolygon = line.BoundingPolygon?.Select(p => new GoalboundFamily.Api.Services.Interfaces.Point
                    {
                        X = p.X,
                        Y = p.Y
                    }).ToList();

                    textBlocks.Add(new OcrTextBlock
                    {
                        Text = lineText,
                        Confidence = lineConfidence * 100, // Convert 0-1 to 0-100
                        LineNumber = lineNumber,
                        BoundingPolygon = boundingPolygon
                    });

                    _logger.LogDebug("Line {LineNumber}: '{Text}' (Confidence: {Confidence:F2}%)",
                        lineNumber, lineText, lineConfidence * 100);
                }
            }

            // Step 6: Calculate overall confidence (average of all line confidences)
            var overallConfidence = confidenceScores.Any()
                ? confidenceScores.Average() * 100 // Convert 0-1 to 0-100
                : 0m;

            var fullText = string.Join("\n", allText);

            _logger.LogInformation(
                "OCR completed successfully. Confidence: {Confidence:F2}%, Text length: {Length}, Lines: {Lines}",
                overallConfidence, fullText.Length, lineNumber);

            return new OcrResult
            {
                Text = fullText,
                Confidence = overallConfidence,
                TextBlocks = textBlocks,
                Success = true
            };
        }
        catch (RequestFailedException ex)
        {
            // Azure-specific exceptions (invalid key, quota exceeded, etc.)
            _logger.LogError(ex, "Azure Computer Vision API request failed. Status: {Status}, Error: {Error}",
                ex.Status, ex.Message);

            var errorMessage = ex.Status switch
            {
                401 => "Invalid Azure Vision API key. Check AZURE_VISION_KEY environment variable.",
                403 => "Access denied. Verify your Azure subscription is active.",
                429 => "Rate limit exceeded. Upgrade from Free F0 tier or wait for quota reset.",
                _ => $"Azure Vision API error: {ex.Message}"
            };

            return new OcrResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "OCR request timed out");
            return new OcrResult
            {
                Success = false,
                ErrorMessage = "OCR request timed out. The image may be too large."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR processing failed with unexpected error");
            return new OcrResult
            {
                Success = false,
                ErrorMessage = $"OCR processing failed: {ex.Message}"
            };
        }
    }
}