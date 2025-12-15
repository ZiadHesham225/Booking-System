using Microsoft.AspNetCore.Http;

namespace Booking_System.Application.Interfaces
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile imageFile, string folderName);
        void DeleteImage(string imageUrl);
    }
}
