namespace CinemaBooking.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(byte[] content, string extension, string containerName);
    void DeleteFile(string fileName, string containerName);
}
