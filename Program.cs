using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0].Equals("--elevated", StringComparison.OrdinalIgnoreCase))
        {
            RunElevated(args.Length > 1 ? args[1] : "", args.Length > 2 ? Texts.Parse(args[2]) : Texts.Detect(), args.Length > 3 ? args[3] : "");
            return;
        }
        if (args.Length > 0 && args[0].Equals("--self-test", StringComparison.OrdinalIgnoreCase))
        {
            try { SelfTest(); }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Self-test failed: " + ex.Message);
                Environment.Exit(1);
            }
            return;
        }
        FontDiscovery.DeleteStaleSessions(TimeSpan.FromDays(2));
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, Texts.Get("Title", Texts.Detect()), MessageBoxButtons.OK, MessageBoxIcon.Error);
        };
        Application.Run(new MainForm(Texts.Detect()));
    }

    static void RunElevated(string manifest, UiLanguage language, string sessionFolder)
    {
        try
        {
            string[] paths = File.ReadAllLines(manifest, Encoding.UTF8).Where(FontDiscovery.IsFont).Where(File.Exists).ToArray();
            InstallResult result = FontInstaller.Install(paths, true);
            MessageBox.Show(result.ToMessage(language, true), Texts.Get("Title", language), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Texts.Get("Title", language), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            TryDelete(manifest);
            FontDiscovery.DeleteSessionFolder(sessionFolder);
        }
    }

    static void SelfTest()
    {
        if (!FontDiscovery.IsFont("a.otf") || !FontDiscovery.IsFont("b.TTF") || FontDiscovery.IsFont("c.txt")) throw new Exception("Font extension test failed.");
        if (!FontDiscovery.IsArchive("a.ZIP") || !FontDiscovery.IsArchive("b.7z") || !FontDiscovery.IsArchive("c.tar.GZ") || !FontDiscovery.IsArchive("d.lzh") || FontDiscovery.IsArchive("e.ttf") || FontDiscovery.IsArchive("f.gz")) throw new Exception("Archive extension test failed.");
        string root = Path.Combine(Path.GetTempPath(), "FontDropTest");
        Directory.CreateDirectory(root);
        if (!FontDiscovery.SafeExtractPath(root, "a/b.otf").StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase)) throw new Exception("Safe path test failed.");
        try { FontDiscovery.SafeExtractPath(root, "../escape.otf"); throw new Exception("ZIP traversal test failed."); } catch (InvalidDataException) { }
        if (!FontDiscovery.IsSessionFolder(FontDiscovery.SessionTemp) || FontDiscovery.IsSessionFolder(root) || FontDiscovery.IsSessionFolder(Path.GetTempPath()))
            throw new Exception("Session folder test failed.");
        if (Texts.Parse("ja") != UiLanguage.Japanese || Texts.Parse("en") != UiLanguage.English || Texts.Get("Title", UiLanguage.English) == "Title")
            throw new Exception("Localization test failed.");
        Directory.Delete(root, true);
        Console.WriteLine("Self-test passed");
    }

    static void TryDelete(string path) { try { if (File.Exists(path)) File.Delete(path); } catch { } }
}
