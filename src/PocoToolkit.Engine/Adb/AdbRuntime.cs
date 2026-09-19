using System.Diagnostics;

namespace PocoToolkit.Engine.Adb;

public sealed class AdbRuntime : IAdbRuntime
{
    public string? GetAdbPath()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "adb",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            var output = process.StandardOutput.ReadLine();

            process.WaitForExit();

            return string.IsNullOrWhiteSpace(output)
                ? null
                : output.Trim();
        }
        catch
        {
            return null;
        }
    }

    public Task<bool> IsInstalledAsync()
    {
        return Task.FromResult(GetAdbPath() is not null);
    }

    public async Task<string?> GetVersionAsync()
    {
        if (GetAdbPath() is null)
            return null;

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "adb",
                    Arguments = "version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();

            await process.WaitForExitAsync();

            foreach (var line in output.Split(Environment.NewLine))
            {
                if (line.StartsWith("Version "))
                    return line.Replace("Version ", "").Trim();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string> ExecuteAsync(string arguments)
    {
        var adbPath = GetAdbPath();

        if (adbPath is null)
            return string.Empty;

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(output))
                return output.Trim();

            return error.Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<string?> GetDeviceSerialAsync()
    {
        var output = await ExecuteAsync("devices");

        if (string.IsNullOrWhiteSpace(output))
            return null;

        foreach (var line in output.Split(Environment.NewLine))
        {
            var parts = line
                .Split('\t', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2 &&
                parts[1].Trim() == "device")
            {
                return parts[0].Trim();
            }
        }

        return null;
    }

    public async Task<string?> GetDeviceModelAsync()
    {
        var serial = await GetDeviceSerialAsync();

        if (string.IsNullOrWhiteSpace(serial))
            return null;

        // Prefer the consumer-facing market name.
        var marketName = await ExecuteAsync(
            $"-s {serial} shell getprop ro.product.marketname");

        if (!string.IsNullOrWhiteSpace(marketName))
            return marketName.Trim();

        // Fallback to Android's model property.
        var model = await ExecuteAsync(
            $"-s {serial} shell getprop ro.product.model");

        return string.IsNullOrWhiteSpace(model)
            ? null
            : model.Trim();
    }
}