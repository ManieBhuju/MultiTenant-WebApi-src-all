using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MultiTenant.Infrastructure.Services;

public class JwtLogStore : IJwtLogStore
{
    private readonly ConcurrentQueue<string> _queue = new();
    private readonly int _max = 200;

    public void AddEntry(string entry)
    {
        _queue.Enqueue(entry);
        while (_queue.Count > _max && _queue.TryDequeue(out _)) { }
    }

    public IReadOnlyList<string> GetEntries() => _queue.ToArray();
}
