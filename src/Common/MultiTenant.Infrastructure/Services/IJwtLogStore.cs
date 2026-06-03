using System.Collections.Generic;

namespace MultiTenant.Infrastructure.Services;

public interface IJwtLogStore
{
    void AddEntry(string entry);
    IReadOnlyList<string> GetEntries();
}
