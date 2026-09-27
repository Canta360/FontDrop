using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

internal sealed class InstallResult
{
    public int Installed;
    public int Skipped;
    public int Failed;
    public readonly List<string> Errors = new List<string>();

    public string ToMessage(UiLanguage language, bool allUsers)
    {
        string scope = Texts.Get(allUsers ? "AllUsersScope" : "UserScope", language);
        string text = scope + Texts.Get("Completed", language) + "\r\n\r\n" +
            Texts.Get("Installed", language) + ": " + Installed + "\r\n" +
            Texts.Get("Skipped", language) + ": " + Skipped + "\r\n" +
            Texts.Get("Failed", language) + ": " + Failed;
        if (Errors.Count > 0) text += "\r\n\r\n" + String.Join("\r\n", Errors.Take(5));
        return text;
    }
}

internal static class FontInstaller
{
    public static InstallResult Install(IEnumerable<string> paths, bool allUsers)
    {
        InstallResult result = new InstallResult();
        string destination = allUsers
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Fonts");
        string registryPath = "Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts";
        Directory.CreateDirectory(destination);
        using (RegistryKey key = (allUsers ? Registry.LocalMachine : Registry.CurrentUser).CreateSubKey(registryPath))
        {
            if (key == null) throw new InvalidOperationException("Could not open the font registry.");
            foreach (string path in paths) InstallOne(path, destination, key, allUsers, result);
        }
        Native.BroadcastFontChange();
        return result;
    }

    static void InstallOne(string source, string destination, RegistryKey key, bool allUsers, InstallResult result)
    {
        string name = Path.GetFileName(source);
        string target = Path.Combine(destination, name);
        bool copied = false;
        bool registered = false;
        try
        {
            if (File.Exists(target) || key.GetValueNames().Any(v => v.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                result.Skipped++;
                return;
            }
            File.Copy(source, target);
            copied = true;
            key.SetValue(name, allUsers ? name : target, RegistryValueKind.String);
            registered = true;
            if (Native.AddFontResourceEx(target, 0, IntPtr.Zero) == 0)
                throw new InvalidOperationException("Windows could not load this font.");
            result.Installed++;
        }
        catch (Exception ex)
        {
            if (registered) TryDeleteRegistryValue(key, name);
            if (copied) TryDelete(target);
            result.Failed++;
            result.Errors.Add(name + ": " + ex.Message);
        }
    }

    static void TryDeleteRegistryValue(RegistryKey key, string name)
    {
        try { key.DeleteValue(name, false); } catch { }
    }

    static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    static class Native
    {
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int AddFontResourceEx(string file, uint flags, IntPtr reserved);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr SendMessageTimeout(IntPtr hwnd, uint msg, UIntPtr wParam, string lParam, uint flags, uint timeout, out UIntPtr result);

        public static void BroadcastFontChange()
        {
            UIntPtr result;
            SendMessageTimeout(new IntPtr(-1), 0x001D, UIntPtr.Zero, null, 2, 1000, out result);
        }
    }
}
