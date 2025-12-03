using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GoalboundFamily.Api.Services.Interfaces;

/// <summary>
/// Service for preprocessing images before OCR
/// </summary>
public interface IImagePreprocessingService
{
    /// <summary>
    /// Preprocess an image to improve OCR accuracy
    /// Applies: grayscale conversion, contrast enhancement, noise reduction, binarization
    /// </summary>
    /// <param name="inputStream">Input image stream</param>
    /// <returns>Preprocessed image ready for OCR</returns>
    Task<Image<Rgba32>> PreprocessImageAsync(Stream inputStream);

    /// <summary>
    /// Save preprocessed image to a stream (for debugging/logging)
    /// </summary>
    /// <param name="image">Preprocessed image</param>
    /// <param name="outputStream">Output stream</param>
    Task SaveImageAsync(Image<Rgba32> image, Stream outputStream);
}
