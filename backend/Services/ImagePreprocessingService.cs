using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using GoalboundFamily.Api.Services.Interfaces;

namespace GoalboundFamily.Api.Services;

/// <summary>
/// Implements image preprocessing for OCR optimization
/// Applies multiple techniques to improve text recognition accuracy
/// </summary>
public class ImagePreprocessingService : IImagePreprocessingService
{
    private readonly ILogger<ImagePreprocessingService> _logger;

    public ImagePreprocessingService(ILogger<ImagePreprocessingService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Preprocess image with aggressive enhancement for receipt OCR
    /// </summary>
    public async Task<Image<Rgba32>> PreprocessImageAsync(Stream inputStream)
    {
        try
        {
            _logger.LogInformation("Starting image preprocessing");

            var image = await Image.LoadAsync<Rgba32>(inputStream);

            _logger.LogInformation("Original image size: {Width}x{Height}", image.Width, image.Height);

            // Apply preprocessing pipeline optimized for receipt OCR
            image.Mutate(ctx =>
            {
                // 1. Auto-orient based on EXIF data (fixes rotated phone photos)
                ctx.AutoOrient();

                // 2. Resize if image is too large (but keep higher resolution for better OCR)
                // Keep aspect ratio, max dimension 3000px (increased from 2000px)
                if (image.Width > 3000 || image.Height > 3000)
                {
                    var scale = Math.Min(3000.0 / image.Width, 3000.0 / image.Height);
                    var newWidth = (int)(image.Width * scale);
                    var newHeight = (int)(image.Height * scale);
                    ctx.Resize(newWidth, newHeight);
                    _logger.LogInformation("Resized image to {Width}x{Height}", newWidth, newHeight);
                }

                // 3. Convert to grayscale (removes color noise)
                ctx.Grayscale();

                // 4. Enhance contrast aggressively for thermal receipts (increased from 1.5f)
                ctx.Contrast(2.0f);

                // 5. Sharpen text edges more aggressively
                ctx.GaussianSharpen(2.0f);

                // 6. Adjust brightness to improve binarization (increased from 1.1f)
                ctx.Brightness(1.3f);

                // 7. Apply binary threshold with adjusted value for better text separation
                // Lower threshold = more text detected (good for faded receipts)
                ctx.BinaryThreshold(0.45f);
            });

            _logger.LogInformation("Image preprocessing completed successfully");

            return image;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during image preprocessing");
            throw new InvalidOperationException("Failed to preprocess image", ex);
        }
    }

    /// <summary>
    /// Save preprocessed image to stream
    /// </summary>
    public async Task SaveImageAsync(Image<Rgba32> image, Stream outputStream)
    {
        await image.SaveAsPngAsync(outputStream);
    }
}
