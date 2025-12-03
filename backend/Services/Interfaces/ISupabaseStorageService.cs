namespace GoalboundFamily.Api.Services.Interfaces;

/// <summary>
/// Service for uploading and managing files in Supabase Storage
/// </summary>
public interface ISupabaseStorageService
{
    /// <summary>
    /// Uploads a file to Supabase Storage
    /// </summary>
    /// <param name="userId">User ID for organizing files</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="fileStream">File content stream</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <returns>Public URL of the uploaded file</returns>
    Task<string> UploadFileAsync(Guid userId, string fileName, Stream fileStream, string contentType);

    /// <summary>
    /// Deletes a file from Supabase Storage
    /// </summary>
    /// <param name="filePath">Path to the file in storage (e.g., userId/filename)</param>
    Task DeleteFileAsync(string filePath);

    /// <summary>
    /// Gets a public URL for a file
    /// </summary>
    /// <param name="filePath">Path to the file in storage</param>
    /// <returns>Public URL to access the file</returns>
    string GetPublicUrl(string filePath);
}
