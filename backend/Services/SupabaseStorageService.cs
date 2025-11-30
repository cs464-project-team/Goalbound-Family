using GoalboundFamily.Api.Services.Interfaces;
using Supabase;

namespace GoalboundFamily.Api.Services;

/// <summary>
/// Implementation of Supabase Storage service
/// Handles file uploads to Supabase Storage buckets
/// </summary>
public class SupabaseStorageService : ISupabaseStorageService
{
    private readonly Client _supabaseClient;
    private readonly ILogger<SupabaseStorageService> _logger;
    private const string BucketName = "receipts";

    public SupabaseStorageService(
        Client supabaseClient,
        ILogger<SupabaseStorageService> logger)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Guid userId, string fileName, Stream fileStream, string contentType)
    {
        try
        {
            // Organize files by user ID: receipts/userId/filename
            var filePath = $"{userId}/{fileName}";

            _logger.LogInformation("Uploading file to Supabase Storage: {FilePath}", filePath);

            // Read stream into byte array
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            // Upload to Supabase Storage
            var uploadResponse = await _supabaseClient.Storage
                .From(BucketName)
                .Upload(fileBytes, filePath, new Supabase.Storage.FileOptions
                {
                    ContentType = contentType,
                    Upsert = false // Don't overwrite existing files
                });

            // Get public URL
            var publicUrl = _supabaseClient.Storage
                .From(BucketName)
                .GetPublicUrl(filePath);

            _logger.LogInformation("File uploaded successfully. Public URL: {Url}", publicUrl);

            return publicUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Supabase Storage: {FileName}", fileName);
            throw new InvalidOperationException($"Failed to upload file to Supabase Storage: {ex.Message}", ex);
        }
    }

    public async Task DeleteFileAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Deleting file from Supabase Storage: {FilePath}", filePath);

            await _supabaseClient.Storage
                .From(BucketName)
                .Remove(new List<string> { filePath });

            _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from Supabase Storage: {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to delete file from Supabase Storage: {ex.Message}", ex);
        }
    }

    public string GetPublicUrl(string filePath)
    {
        return _supabaseClient.Storage
            .From(BucketName)
            .GetPublicUrl(filePath);
    }
}
