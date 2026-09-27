using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

internal sealed class FontEntry
{
    public readonly string SourcePath;
    public readonly string DisplayName;

    public FontEntry(string sourcePath, string displayName)
    {
        SourcePath = sourcePath;
        DisplayName = displayName;
    }

    public override string ToString() { return DisplayName; }
}

internal static class FontDiscovery
{
    static readonly string[] Extensions = { ".ttf", ".otf", ".ttc", ".fon", ".fnt" };
    public static readonly string TempRoot = Path.Combine(Path.GetTempPath(), "FontDrop");
    public static readonly string SessionTemp = Path.Combine(TempRoot, Guid.NewGuid().ToString("N"));

    public static bool IsFont(string path)
    {
        return Extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
    }

    public static List<FontEntry> Inspect(string source)
    {
        if (IsFont(source)) return new List<FontEntry> { new FontEntry(source, Path.GetFileName(source)) };
        if (Directory.Exists(source)) return InspectFolder(source);
        if (source.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) return InspectZip(source);
        return new List<FontEntry>();
    }

    static List<FontEntry> InspectFolder(string folder)
    {
        List<FontEntry> result = new List<FontEntry>();
        string root = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string name = new DirectoryInfo(folder).Name;
        foreach (string path in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories).Where(IsFont))
            result.Add(new FontEntry(path, name + "\\" + path.Substring(root.Length)));
        return result;
    }

    static List<FontEntry> InspectZip(string archivePath)
    {
        List<FontEntry> result = new List<FontEntry>();
        string temp = Path.Combine(SessionTemp, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        using (ZipArchive archive = ZipFile.OpenRead(archivePath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (String.IsNullOrEmpty(entry.Name) || !IsFont(entry.Name)) continue;
                string output = SafeExtractPath(temp, entry.FullName);
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                entry.ExtractToFile(output, true);
                result.Add(new FontEntry(output, Path.GetFileName(archivePath) + "\\" + entry.FullName.Replace('/', '\\')));
            }
        }
        return result;
    }

    public static string SafeExtractPath(string root, string entryName)
    {
        string fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string fullPath = Path.GetFullPath(Path.Combine(root, entryName.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("ZIP entry is outside its extraction folder.");
        return fullPath;
    }

    public static bool IsSessionFolder(string path)
    {
        if (String.IsNullOrEmpty(path)) return false;
        try
        {
            string full = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            return String.Equals(Path.GetDirectoryName(full), Path.GetFullPath(TempRoot).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception) { return false; }
    }

    public static void DeleteSessionFolder(string path)
    {
        if (!IsSessionFolder(path)) return;
        try
        {
            if (!Directory.Exists(path)) return;
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) return;
            Directory.Delete(path, true);
        }
        catch { }
    }

    public static void DeleteStaleSessions(TimeSpan age)
    {
        try
        {
            if (!Directory.Exists(TempRoot)) return;
            foreach (string folder in Directory.GetDirectories(TempRoot))
                if (DateTime.UtcNow - Directory.GetLastWriteTimeUtc(folder) > age) DeleteSessionFolder(folder);
        }
        catch { }
    }
}
