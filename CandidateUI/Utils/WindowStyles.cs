using System;
using System.Runtime.InteropServices;

namespace ScjWin11UI.Utils;

public static class WindowStyles
{
    private const int GWL_EXSTYLE = -20;

    private const int WS_EX_TOOLWINDOW = 0x00000080;   // hide from Alt-Tab
    private const int WS_EX_NOACTIVATE = 0x08000000;   // do not activate on click
    private const int WS_EX_TOPMOST = 0x00000008;

    private const int SW_HIDE = 0;
    private const int SW_SHOWNOACTIVATE = 4;

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;

    private static readonly IntPtr HWND_TOPMOST = new(-1);

    public static void ApplyCandidateWindowStyles(IntPtr hwnd)
    {
        // Add: toolwindow + noactivate + topmost.
        var ex = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
        ex |= WS_EX_TOOLWINDOW;
        ex |= WS_EX_NOACTIVATE;
        ex |= WS_EX_TOPMOST;
        SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(ex));

        // Ensure topmost without activation.
        SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
    }

    public static void ShowNoActivate(IntPtr hwnd)
        => ShowWindow(hwnd, SW_SHOWNOACTIVATE);

    public static void Hide(IntPtr hwnd)
        => ShowWindow(hwnd, SW_HIDE);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags);

    private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        if (IntPtr.Size == 8) return GetWindowLongPtr64(hWnd, nIndex);
        return GetWindowLong32(hWnd, nIndex);
    }

    private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 8) return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        return SetWindowLong32(hWnd, nIndex, dwNewLong);
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
    private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
}
