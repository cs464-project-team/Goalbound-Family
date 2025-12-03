namespace GoalboundFamily.Api.Services.Interfaces;

/// <summary>
/// Result of OCR processing
/// </summary>
public class OcrResult
{
    /// <summary>
    /// Full extracted text from the image
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Average confidence score (0-100)
    /// </summary>
    public decimal Confidence { get; set; }

    /// <summary>
    /// Individual text blocks with confidence scores
    /// </summary>
    public List<OcrTextBlock> TextBlocks { get; set; } = new();

    /// <summary>
    /// True if OCR processing was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if processing failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Individual text block from OCR
/// </summary>
public class OcrTextBlock
{
    public string Text { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public int LineNumber { get; set; }

    /// <summary>
    /// Bounding polygon points (X,Y coordinates)
    /// Azure returns 4 points for each line: top-left, top-right, bottom-right, bottom-left
    /// </summary>
    public List<Point>? BoundingPolygon { get; set; }
}

/// <summary>
/// Represents a 2D point (X, Y coordinates)
/// Used for bounding box polygons from OCR
/// </summary>
public class Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

/// <summary>
/// Service for performing OCR on images
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Perform OCR on an image stream
    /// </summary>
    /// <param name="imageStream">Image to process</param>
    /// <returns>OCR result with extracted text and confidence</returns>
    Task<OcrResult> ProcessImageAsync(Stream imageStream);
}
