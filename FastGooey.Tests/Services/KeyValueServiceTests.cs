using FastGooey.Services;
using FastGooey.Tests.Support;
using NodaTime;

namespace FastGooey.Tests.Services;

public class KeyValueServiceTests
{
    [Fact]
    public async Task SetValueForKey_InsertsNewValue()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 3, 1, 10, 0));
        await using var context = TestDbContextFactory.Create(clock);
        var service = new KeyValueService(context);

        await service.SetValueForKey("theme", "sunrise");

        var value = await service.GetValueForKey("theme");

        Assert.Equal("sunrise", value);
    }

    [Fact]
    public async Task SetValueForKey_UpdatesExistingValue()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 3, 1, 10, 0));
        await using var context = TestDbContextFactory.Create(clock);
        var service = new KeyValueService(context);

        await service.SetValueForKey("theme", "sunrise");
        await service.SetValueForKey("theme", "midnight");

        var value = await service.GetValueForKey("theme");

        Assert.Equal("midnight", value);
    }
}
