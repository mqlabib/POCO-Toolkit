using System.Linq;

using PocoToolkit.Contracts.Abstractions;

namespace PocoToolkit.Engine.Adb;

public sealed class AdbService : IAdbService
{
    private readonly IAdbRuntime _runtime;

    public AdbService(IAdbRuntime runtime)
    {
        _runtime = runtime;
    }

    public Task<bool> IsInstalledAsync()
        => _runtime.IsInstalledAsync();

    public Task<string?> GetVersionAsync()
        => _runtime.GetVersionAsync();

    public string? GetPath()
        => _runtime.GetAdbPath();

    public async Task<bool> IsDeviceConnectedAsync()
    {
        var output = await _runtime.ExecuteAsync("devices");

        if (string.IsNullOrWhiteSpace(output))
            return false;

        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Any(line => line.TrimEnd().EndsWith("device"));
    }
}