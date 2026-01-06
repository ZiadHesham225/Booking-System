namespace Booking_System.Application.Common
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> Success(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> Failure(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors
            };
        }
    }

    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse Success(string? message = null)
        {
            return new ApiResponse
            {
                IsSuccess = true,
                Message = message
            };
        }

        public static ApiResponse Failure(string message, List<string>? errors = null)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = message,
                Errors = errors
            };
        }
    }
}
