using FastGooey.Database;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace FastGooey.Tests.Support;

public sealed class TestClock : IClock
{
    public TestClock(Instant instant)
    {
        CurrentInstant = instant;
    }

    public Instant CurrentInstant { get; private set; }

    public Instant GetCurrentInstant()
    {
        return CurrentInstant;
    }

    public void Set(Instant instant)
    {
        CurrentInstant = instant;
    }
}

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create(TestClock clock)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options, clock);
    }
}
