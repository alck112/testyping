using Microsoft.UI.Xaml;

namespace ScjWin11UI;

public partial class App : Application
{
    private CandidateWindow? _candidateWindow;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Create once at startup, then keep hidden.
        // This avoids stealing focus later while the user is typing.
        _candidateWindow = new CandidateWindow();
        _candidateWindow.Activate();
        _candidateWindow.HideNoActivate();
    }
}
