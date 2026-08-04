namespace PocoToolkit.Contracts.Abstractions;

public interface IAdbService
{
    Task<bool> IsInstalledAsync();

    Task<string?> GetVersionAsync();

    string? GetPath();
}