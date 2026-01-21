using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ScjPortableShared;

namespace ScjLauncher;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        var baseDir = AppContext.BaseDirectory;
        var engineDll = Path.Combine(baseDir, "ScjTsfEngine.dll");
        var uiExe = Path.Combine(baseDir, "ScjWin11UI.exe");

        if (args.Length > 0)
        {
            var cmd = args[0].ToLowerInvariant();
            if (cmd is "/install" or "-install")
            {
                TsfRegistration.InstallPerUser(engineDll);
                StartUi(uiExe);
                return;
            }
            if (cmd is "/uninstall" or "-uninstall")
            {
                TsfRegistration.UninstallPerUser();
                return;
            }
            if (cmd is "/ui" or "-ui")
            {
                StartUi(uiExe);
                return;
            }
        }

        var form = new Form
        {
            Text = "SCJ Portable (Win11 UI) - Launcher",
            Width = 560,
            Height = 300,
            StartPosition = FormStartPosition.CenterScreen
        };

        var info = new Label
        {
            Left = 14,
            Top = 14,
            Width = 520,
            Height = 95,
            Text = "Windows 輸入法（TSF）無法真正『零註冊』就使用。\n" +
                   "但可以做到像免安裝：用『使用者層級』註冊（不需管理員），一鍵啟用。\n" +
                   "第一次按 Install 會：註冊輸入法 + 自動啟動 Win11 候選窗 UI。\n" +
                   "然後按 Open Settings 去把鍵盤加入：快倉 (SCJ) - Prototype。"
        };

        var btnInstall = new Button { Left = 14, Top = 120, Width = 140, Height = 34, Text = "Install + Start UI" };
        var btnStartUi  = new Button { Left = 164, Top = 120, Width = 100, Height = 34, Text = "Start UI" };
        var btnSettings = new Button { Left = 274, Top = 120, Width = 130, Height = 34, Text = "Open Settings" };
        var btnUninstall= new Button { Left = 414, Top = 120, Width = 110, Height = 34, Text = "Uninstall" };

        var status = new TextBox
        {
            Left = 14,
            Top = 170,
            Width = 510,
            Height = 70,
            Multiline = true,
            ReadOnly = true
        };

        btnInstall.Click += (_, __) =>
        {
            try
            {
                TsfRegistration.InstallPerUser(engineDll);
                StartUi(uiExe);
                status.Text = "Installed (per-user) and UI started. Now click 'Open Settings' and add keyboard: 快倉 (SCJ) - Prototype.";
            }
            catch (Exception ex)
            {
                status.Text = "Install failed: " + ex.Message;
            }
        };

        btnStartUi.Click += (_, __) =>
        {
            try { StartUi(uiExe); status.Text = "UI started."; }
            catch (Exception ex) { status.Text = "Start UI failed: " + ex.Message; }
        };

        btnSettings.Click += (_, __) =>
        {
            try
            {
                Process.Start(new ProcessStartInfo("ms-settings:timeandlanguage-language") { UseShellExecute = true });
                status.Text = "Opened Settings. Go to Chinese (Traditional) -> Keyboard -> Add a keyboard -> 快倉 (SCJ) - Prototype";
            }
            catch (Exception ex)
            {
                status.Text = "Open Settings failed: " + ex.Message;
            }
        };

        btnUninstall.Click += (_, __) =>
        {
            try
            {
                TsfRegistration.UninstallPerUser();
                status.Text = "Uninstalled (per-user). Some systems may require logoff/restart to refresh list.";
            }
            catch (Exception ex) { status.Text = "Uninstall failed: " + ex.Message; }
        };

        form.Controls.Add(info);
        form.Controls.Add(btnInstall);
        form.Controls.Add(btnStartUi);
        form.Controls.Add(btnSettings);
        form.Controls.Add(btnUninstall);
        form.Controls.Add(status);

        Application.Run(form);
    }

    private static void StartUi(string uiExe)
    {
        if (!File.Exists(uiExe))
            throw new FileNotFoundException("ScjWin11UI.exe not found next to launcher.", uiExe);
        Process.Start(new ProcessStartInfo(uiExe) { UseShellExecute = true });
    }
}
