namespace PocoToolkit.Engine.Fastboot;

public interface IFastbootRuntime
{
    Task<string> ExecuteAsync(string arguments);
}