using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;

using ScjWin11UI.Models;
using ScjWin11UI.Services;
using ScjWin11UI.Utils;

namespace ScjWin11UI;

public sealed partial class CandidateWindow : Window
{
    public CandidateViewModel ViewModel { get; } = new();

    private readonly DispatcherQueue _ui;
    private readonly ImePipeServer _pipe;
    private readonly IntPtr _hwnd;
    private readonly AppWindow _appWindow;

    public CandidateWindow()
    {
        InitializeComponent();

        // Optional: make ThemeShadow render by setting receivers.
        // Safe even if shadow does not render on older OS versions.
        try
        {
            if (Card.Shadow is Microsoft.UI.Xaml.Media.ThemeShadow ts)
                ts.Receivers.Add(Root);
        }
        catch { }


        _ui = DispatcherQueue.GetForCurrentThread();

        _hwnd = WindowNative.GetWindowHandle(this);
        _appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(_hwnd));

        // Window behavior & style: topmost, borderless, no-activate, no Alt-Tab.
        WindowStyles.ApplyCandidateWindowStyles(_hwnd);

        if (_appWindow.Presenter is OverlappedPresenter p)
        {
            p.IsAlwaysOnTop = true;
            p.IsResizable = false;
            p.IsMinimizable = false;
            p.IsMaximizable = false;
            p.SetBorderAndTitleBar(false, false);
        }

        // Start pipe server (UI is server; IME engine connects as client).
        _pipe = new ImePipeServer(
            pipeName: "scj.ui.v1",
            onMessage: msg => _ui.TryEnqueue(() => HandleMessage(msg))
        );

        _pipe.Start();
    }

    public void ShowNoActivate()
        => WindowStyles.ShowNoActivate(_hwnd);

    public void HideNoActivate()
        => WindowStyles.Hide(_hwnd);

    private void HandleMessage(ImeToUiMessage msg)
    {
        switch (msg.Type)
        {
            case "hide":
                HideNoActivate();
                break;

            case "update":
            case "show":
                if (msg.State is null) return;

                ViewModel.Composition = msg.State.Composition ?? "";
                ViewModel.SetCandidates(msg.State.Candidates);
                ViewModel.SelectedIndex = Math.Clamp(msg.State.SelectedIndex, 0, Math.Max(0, ViewModel.Candidates.Count - 1));
                ViewModel.Hint = msg.State.Hint ?? "Click a candidate to select";

                if (msg.State.Caret is not null)
                {
                    // Engine should send screen coordinates in physical pixels.
                    // We'll move the window near caret and clamp it on-screen.
                    PositionNearCaret(msg.State.Caret.Value);
                }

                if (msg.Type == "show")
                    ShowNoActivate();

                break;

            default:
                // ignore unknown
                break;
        }
    }

    private void PositionNearCaret(ScreenRect caret)
    {
        // Basic positioning strategy: below caret with a small offset.
        // Engine can fine-tune by sending different caret rects or offsets.
        const int offsetY = 8;
        var x = caret.X;
        var y = caret.Y + caret.Height + offsetY;

        // Optional: clamp to nearest display.
        var pos = new Windows.Graphics.PointInt32(x, y);
        _appWindow.Move(pos);
    }

    private async void CandidateList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is CandidateItem item)
        {
            await _pipe.SendAsync(new UiToImeMessage
            {
                Type = "select",
                Index = item.Index
            });
        }
    }

    private void CandidateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Keep selection UI in sync; no-op otherwise.
        // (Keyboard selection is expected to be handled by the engine.)
    }
}
