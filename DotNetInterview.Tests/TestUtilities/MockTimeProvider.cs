using DotNetInterview.API.Services;

namespace DotNetInterview.Tests.TestUtilities
{
    public class MockTimeProvider : ITimeProvider
    {
        public DateTime UtcNow { get; set; }
    }
}
