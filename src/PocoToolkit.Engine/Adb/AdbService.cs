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

    public Task<bool> IsDeviceConnectedAsync()
        => IsDeviceConnectedInternalAsync();

    private async Task<bool> IsDeviceConnectedInternalAsync()
    {
        var output = await _runtime.ExecuteAsync("devices");

        if (string.IsNullOrWhiteSpace(output))
            return false;

        foreach (var line in output.Split(Environment.NewLine))
        {
            var trimmed = line.Trim();

            if (trimmed.EndsWith("\tdevice", StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}