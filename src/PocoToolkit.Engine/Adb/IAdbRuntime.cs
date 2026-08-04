namespace PocoToolkit.Engine.Adb;

public interface IAdbRuntime
{
    Task<bool> IsInstalledAsync();

    Task<string?> GetVersionAsync();

    string? GetAdbPath();
}