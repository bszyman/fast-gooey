using FastGooey.Models;
using FastGooey.Tests.Support;
using NodaTime;

namespace FastGooey.Tests.Database;

public class ApplicationDbContextTests
{
    [Fact]
    public void SaveChanges_SetsWorkspaceTimestampsOnAdd()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var context = TestDbContextFactory.Create(clock);

        var workspace = new Workspace
        {
            Name = "Acme Workspace",
            Slug = "acme-workspace"
        };

        context.Workspaces.Add(workspace);
        context.SaveChanges();

        Assert.Equal(clock.CurrentInstant, workspace.CreatedAt);
        Assert.Equal(clock.CurrentInstant, workspace.UpdatedAt);
    }

    [Fact]
    public void SaveChanges_UpdatesWorkspaceUpdatedAtOnModify()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 1, 1, 12, 0));
        using var context = TestDbContextFactory.Create(clock);

        var workspace = new Workspace
        {
            Name = "Acme Workspace",
            Slug = "acme-workspace"
        };

        context.Workspaces.Add(workspace);
        context.SaveChanges();

        var createdAt = workspace.CreatedAt;
        clock.Set(Instant.FromUtc(2024, 1, 2, 9, 30));

        workspace.Name = "Acme Workspace Updated";
        context.SaveChanges();

        Assert.Equal(createdAt, workspace.CreatedAt);
        Assert.Equal(clock.CurrentInstant, workspace.UpdatedAt);
    }

    [Fact]
    public void SaveChanges_SetsUserAndInterfaceTimestampsOnAdd()
    {
        var clock = new TestClock(Instant.FromUtc(2024, 2, 1, 8, 15));
        using var context = TestDbContextFactory.Create(clock);

        var workspace = new Workspace
        {
            Name = "Team Workspace",
            Slug = "team-workspace"
        };

        var user = new ApplicationUser
        {
            UserName = "user@example.com",
            Email = "user@example.com"
        };

        var gooeyInterface = new GooeyInterface
        {
            Workspace = workspace,
            Name = "Primary Interface",
            Platform = "mac"
        };

        context.Workspaces.Add(workspace);
        context.Users.Add(user);
        context.GooeyInterfaces.Add(gooeyInterface);
        context.SaveChanges();

        Assert.Equal(clock.CurrentInstant, user.CreatedAt);
        Assert.Equal(clock.CurrentInstant, user.UpdatedAt);
        Assert.Equal(clock.CurrentInstant, gooeyInterface.CreatedAt);
        Assert.Equal(clock.CurrentInstant, gooeyInterface.UpdatedAt);
    }
}
