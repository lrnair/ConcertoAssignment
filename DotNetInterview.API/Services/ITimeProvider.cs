namespace DotNetInterview.API.Services
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}