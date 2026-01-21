using System;
using System.Windows.Forms;
using ScjPortableShared;

namespace ScjUninstall;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            TsfRegistration.UninstallPerUser();
            MessageBox.Show(
                "已移除（使用者層級）快倉輸入法註冊。\n\n若系統輸入法清單仍顯示，請登出或重新開機讓 TSF 刷新。",
                "SCJ Uninstall",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Uninstall failed: " + ex.Message, "SCJ Uninstall", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
