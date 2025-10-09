using FastGooey.Database;
using FastGooey.Models;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Services;

public interface IKeyValueService
{
    public Task SetValueForKey(string key, string value);
    public Task<string?> GetValueForKey(string key);
}

public class KeyValueService(ApplicationDbContext context): IKeyValueService
{
    public async Task SetValueForKey(string key, string value)
    {
        var existing = await context.KeyValueStores.FirstOrDefaultAsync(x => x.Key == key);
        
        if (existing != null)
        {
            existing.Value = value;
        }
        else
        {
            var kvs = new KeyValueStore
            {
                Key = key,
                Value = value
            };
            
            context.KeyValueStores.Add(kvs);
        }
        
        await context.SaveChangesAsync();
    }

    public async Task<string?> GetValueForKey(string key)
    {
        var kvs = await context
            .KeyValueStores
            .FirstOrDefaultAsync(x => x.Key == key);
        
        return kvs?.Value;
    }
}
