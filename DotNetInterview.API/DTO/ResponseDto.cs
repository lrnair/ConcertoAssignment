namespace DotNetInterview.API.DTO;

public record ResponseDto
{
    public Guid Id { get; set; }
    public string Message { get; set; }
}
