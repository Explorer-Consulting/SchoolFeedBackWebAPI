public abstract class BaseResponseDTO
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    protected BaseResponseDTO(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}