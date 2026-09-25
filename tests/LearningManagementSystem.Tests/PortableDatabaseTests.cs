using LearningManagementSystem.Persistence.Context;
using LearningManagementSystem.Persistence.Seed;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Tests;

public class PortableDatabaseTests
{
    [Fact]
    public async Task FreshSqliteFileCreatesAndKeepsAiCourseAcrossConnections()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"academy-{Guid.NewGuid():N}.db");
        var connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connectionString)
            .Options;

        try
        {
            await using (var db = new ApplicationDbContext(options))
            {
                await db.Database.EnsureCreatedAsync();
                await DataSeeder.SeedSampleQuizDataAsync(db);
                Assert.Equal(3, await db.Lessons.CountAsync());
                Assert.Equal(3, await db.Questions.CountAsync());
                Assert.Single(await db.Assignments.ToListAsync());
            }

            await using (var reopened = new ApplicationDbContext(options))
            {
                Assert.Equal("Getting Started with AI", (await reopened.Courses.SingleAsync()).Title);
                await DataSeeder.SeedSampleQuizDataAsync(reopened);
                Assert.Equal(1, await reopened.Courses.CountAsync());
            }
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }
}
