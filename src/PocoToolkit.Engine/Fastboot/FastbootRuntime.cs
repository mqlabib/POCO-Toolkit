using System.Diagnostics;

namespace PocoToolkit.Engine.Fastboot;

public sealed class FastbootRuntime : IFastbootRuntime
{
    public async Task<string> ExecuteAsync(string arguments)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "fastboot",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var output = await outputTask;
            var error = await errorTask;

            var combined = string.Join(
                Environment.NewLine,
                new[] { output, error }
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value.Trim()));

            return combined;
        }
        catch
        {
            return string.Empty;
        }
    }
}