using System;
using System.IO;
using Microsoft.Win32;

namespace ScjPortableShared;

public static class TsfRegistration
{
    public const string ClsidTextService = "{F34E9F5B-8A6E-4ED7-9AE5-8C7C7B6463D6}";
    public const string ProfileGuid = "{ACD4A1D2-7B1E-4A5A-B1D0-9E7D1C5D6AA1}";
    public const int LangId = 0x0404; // zh-TW

    public static void InstallPerUser(string engineDllPath)
    {
        if (!File.Exists(engineDllPath))
            throw new FileNotFoundException("ScjTsfEngine.dll not found next to exe.", engineDllPath);

        using var clsidKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\CLSID\{ClsidTextService}");
        clsidKey!.SetValue(null, "SCJ TSF Text Service (Per-User)", RegistryValueKind.String);

        using var inproc = clsidKey.CreateSubKey("InprocServer32");
        inproc!.SetValue(null, engineDllPath, RegistryValueKind.String);
        inproc.SetValue("ThreadingModel", "Both", RegistryValueKind.String);

        dynamic profiles = Activator.CreateInstance(Type.GetTypeFromProgID("Msctf.InputProcessorProfiles")!)!;
        profiles.EnableLanguageProfile(new Guid(ClsidTextService), LangId, new Guid(ProfileGuid), "快倉 (SCJ) - Prototype", 0);
    }

    public static void UninstallPerUser()
    {
        try { Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\CLSID\{ClsidTextService}", false); } catch { }
    }
}
