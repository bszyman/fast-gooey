using FastGooey.Database;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Services;

public class DatabaseInitializer(IServiceProvider services)
{
    public void Initialize()
    {
        using (var scope = services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();
        }
    }
}