using DotNetInterview.API;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DotNetInterview.Tests
{
    public class SeedDataTests
    {
        private DataContext _dataContext;

        [SetUp]
        public void Setup()
        {
            var connection = new SqliteConnection("Data Source=DotNetInterview;Mode=Memory;Cache=Shared");
            connection.Open();
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(connection)
                .Options;
            _dataContext = new DataContext(options);
        }

        [Test]
        public void Example_to_ensure_dbcontext_has_seed_data()
        {
            Assert.That(_dataContext.Items.Count(), Is.EqualTo(3));
        }
    }
}
