using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

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

internal sealed class ArchiveProblem
{
    public readonly string DisplayName;
    public readonly string Reason;
    public readonly string Detail;

    public ArchiveProblem(string displayName, string reason, string detail)
    {
        DisplayName = displayName;
        Reason = reason;
        Detail = detail;
    }
}

internal sealed class DiscoveryResult
{
    public readonly List<FontEntry> Fonts = new List<FontEntry>();
    public readonly List<ArchiveProblem> Problems = new List<ArchiveProblem>();
}

internal sealed class ArchiveToolMissingException : Exception { }

internal static class FontDiscovery
{
    static readonly string[] Extensions = { ".ttf", ".otf", ".ttc", ".fon", ".fnt" };
    // ZIP is read by the .NET Framework; everything else by the tar.exe (libarchive) that comes with Windows 11.
    public static readonly string[] ArchiveExtensions =
    {
        ".zip", ".7z", ".rar", ".tar", ".tar.gz", ".tgz", ".tar.bz2", ".tbz2", ".tbz", ".tar.xz", ".txz",
        ".tar.zst", ".tzst", ".lzh", ".lha", ".cab"
    };
    // An archive inside an archive inside an archive is opened; anything deeper is reported and skipped.
    public const int MaxArchiveDepth = 3;
    static readonly TimeSpan ExtractTimeout = TimeSpan.FromMinutes(5);
    public static readonly string TempRoot = Path.Combine(Path.GetTempPath(), "FontDrop");
    public static readonly string SessionTemp = Path.Combine(TempRoot, Guid.NewGuid().ToString("N"));

    public static bool IsFont(string path)
    {
        return Extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsArchive(string path)
    {
        return ArchiveExtensions.Any(extension => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
    }

    public static DiscoveryResult Inspect(string source)
    {
        DiscoveryResult result = new DiscoveryResult();
        if (Directory.Exists(source)) CollectFolder(source, new DirectoryInfo(source).Name, 0, result);
        else CollectFile(source, Path.GetFileName(source), 0, result);
        return result;
    }

    static void CollectFile(string path, string displayName, int depth, DiscoveryResult result)
    {
        if (IsFont(path))
        {
            result.Fonts.Add(new FontEntry(path, displayName));
            return;
        }
        if (!IsArchive(path)) return;
        if (depth >= MaxArchiveDepth)
        {
            result.Problems.Add(new ArchiveProblem(displayName, "ArchiveTooDeep", null));
            return;
        }
        string temp = Path.Combine(SessionTemp, Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(temp);
            if (Path.GetExtension(path).Equals(".zip", StringComparison.OrdinalIgnoreCase)) ExtractZip(path, temp);
            else ExtractWithTar(path, temp);
        }
        catch (ArchiveToolMissingException)
        {
            TryDeleteFolder(temp);
            result.Problems.Add(new ArchiveProblem(displayName, "TarUnavailable", null));
            return;
        }
        catch (Exception ex)
        {
            TryDeleteFolder(temp);
            result.Problems.Add(new ArchiveProblem(displayName, "ArchiveFailed", ex.Message));
            return;
        }
        CollectFolder(temp, displayName, depth + 1, result);
    }

    static void CollectFolder(string folder, string displayName, int depth, DiscoveryResult result)
    {
        string root = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        foreach (string path in EnumerateFiles(folder))
            if (IsFont(path) || IsArchive(path))
                CollectFile(path, displayName + "\\" + path.Substring(root.Length), depth, result);
    }

    // Walks a folder in name order without following junctions or symbolic links.
    static IEnumerable<string> EnumerateFiles(string folder)
    {
        Stack<string> pending = new Stack<string>();
        pending.Push(folder);
        while (pending.Count > 0)
        {
            string current = pending.Pop();
            string[] files, folders;
            try
            {
                files = Directory.GetFiles(current);
                folders = Directory.GetDirectories(current);
            }
            catch (UnauthorizedAccessException) { continue; }
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            foreach (string file in files)
                if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) == 0) yield return file;
            Array.Sort(folders, StringComparer.OrdinalIgnoreCase);
            for (int i = folders.Length - 1; i >= 0; i--)
                if ((File.GetAttributes(folders[i]) & FileAttributes.ReparsePoint) == 0) pending.Push(folders[i]);
        }
    }

    static void ExtractZip(string archivePath, string destination)
    {
        try { ExtractZipWithFramework(archivePath, destination); }
        catch (Exception ex)
        {
            // The .NET Framework cannot read some ZIPs, such as Deflate64 ones made by Windows for large files.
            if (!(ex is InvalidDataException || ex is NotSupportedException)) throw;
            ResetFolder(destination);
            try { ExtractWithTar(archivePath, destination); }
            catch (ArchiveToolMissingException) { throw ex; }
        }
    }

    static void ExtractZipWithFramework(string archivePath, string destination)
    {
        using (ZipArchive archive = ZipFile.OpenRead(archivePath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (String.IsNullOrEmpty(entry.Name) || !(IsFont(entry.Name) || IsArchive(entry.Name))) continue;
                string output = SafeExtractPath(destination, entry.FullName);
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                entry.ExtractToFile(output, true);
            }
        }
    }

    static void ExtractWithTar(string archivePath, string destination)
    {
        string tar = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "tar.exe");
        if (!File.Exists(tar)) throw new ArchiveToolMissingException();
        string error = RunTool(tar, "-x -f " + Quote(archivePath) + " -C " + Quote(destination));
        if (error != null) throw new InvalidDataException(error);
        EnsureInside(destination);
    }

    // Returns null on success, or the first line of the tool's error output.
    static string RunTool(string exe, string arguments)
    {
        ProcessStartInfo info = new ProcessStartInfo(exe, arguments)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using (Process process = Process.Start(info))
        {
            process.StandardInput.Close();
            Task<string> output = process.StandardOutput.ReadToEndAsync();
            Task<string> error = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit((int)ExtractTimeout.TotalMilliseconds))
            {
                try { process.Kill(); } catch { }
                return "Timed out.";
            }
            if (process.ExitCode == 0) return null;
            string[] lines = (error.Result + "\n" + output.Result).Split('\n').Select(l => l.Trim())
                .Where(l => l.Length > 0 && !l.StartsWith("tar.exe: Error exit delayed", StringComparison.Ordinal) && !l.StartsWith("Sub items Errors", StringComparison.Ordinal))
                .ToArray();
            string line = lines.FirstOrDefault(l => l.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0 || l.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0)
                ?? lines.FirstOrDefault();
            return line ?? ("Exit code " + process.ExitCode + ".");
        }
    }

    static void EnsureInside(string destination)
    {
        string root = Path.GetFullPath(destination).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        foreach (string path in EnumerateFiles(destination))
            if (!Path.GetFullPath(path).StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Archive entry is outside its extraction folder.");
    }

    static string Quote(string value) { return "\"" + value + "\""; }

    static void ResetFolder(string folder)
    {
        if (Directory.Exists(folder)) Directory.Delete(folder, true);
        Directory.CreateDirectory(folder);
    }

    static void TryDeleteFolder(string folder)
    {
        try { if (Directory.Exists(folder)) Directory.Delete(folder, true); } catch { }
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
