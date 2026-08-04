using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using PocoToolkit.Engine;

namespace PocoToolkit.App.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ToolkitEngine _engine;

    [ObservableProperty]
    private string adbStatus = "Checking...";

    [ObservableProperty]
    private string adbVersion = "";

    [ObservableProperty]
    private string adbPath = "";

    public MainViewModel(ToolkitEngine engine)
    {
        _engine = engine;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        if (await _engine.IsAdbInstalledAsync())
        {
            AdbStatus = "✔ Installed";
            AdbVersion = await _engine.GetAdbVersionAsync() ?? "";
            AdbPath = _engine.GetAdbPath() ?? "";
        }
        else
        {
            AdbStatus = "❌ Not Installed";
            AdbVersion = "";
            AdbPath = "";
        }
    }
}