using System;
using Microsoft.UI.Xaml;

namespace ScjWin11UI;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();
        Application.Start(_ => new App());
    }
}
