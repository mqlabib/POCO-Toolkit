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
}