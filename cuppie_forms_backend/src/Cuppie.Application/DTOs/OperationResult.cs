namespace Cuppie.Application.DTOs
{
    public class OperationResult<T>
    {
        public bool IsSuccess { get; set; }        
        public T? Data { get; set; }
        public string ErrorMessage { get; set; } = null!;
        public ErrorCode ErrorCode { get; set; }

        
        public static OperationResult<T> Success(T? data)
        {
            return new OperationResult<T> { IsSuccess = true, Data = data };
        }

        public static OperationResult<T> Failure(string errorMessage, ErrorCode errorCode)
        {
            return new OperationResult<T> { IsSuccess = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
        }
    }

    public enum ErrorCode
    {
        UnknownError = 1,
        ValidationError = 2,
        Unauthorized = 3,
        Conflict = 4,
        BadRequest = 5,
        NotFound = 6
    }
}
