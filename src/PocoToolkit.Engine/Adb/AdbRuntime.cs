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
}