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
            string output = await ExecuteAsync("version");

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
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "adb",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync();

        await process.WaitForExitAsync();

        return output;
    }
}