namespace cuppie.DTO
{
    public class OperationResult<T>
    {
        public bool IsSuccess { get; set; }        
        public T Data { get; set; }
        public string? ErrorMessage { get; set; }
        public ErrorCode? ErrorCode { get; set; }

        // Конструкторы для удобства
        public static OperationResult<T> Success(T data)
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
        InvalidInput = 2,
        NotFound = 3,
        Unauthorized = 4,
        Conflict = 5,
        InternalServerError = 6
    }
}
