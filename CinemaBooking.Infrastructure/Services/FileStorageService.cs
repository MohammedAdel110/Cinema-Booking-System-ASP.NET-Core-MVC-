namespace CinemaBooking.Infrastructure.Services;

using CinemaBooking.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System.IO;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public void DeleteFile(string fileName, string containerName)
    {
        var filePath = Path.Combine(_env.WebRootPath, containerName, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public async Task<string> SaveFileAsync(byte[] content, string extension, string containerName)
    {
        var fileName = $"{Guid.NewGuid()}{extension}";
        var folderPath = Path.Combine(_env.WebRootPath, containerName);
        
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var filePath = Path.Combine(folderPath, fileName);
        await File.WriteAllBytesAsync(filePath, content);

        return fileName;
    }
}
