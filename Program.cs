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
            RunElevated(args.Length > 1 ? args[1] : "", args.Length > 2 ? Texts.Parse(args[2]) : Texts.Detect());
            return;
        }
        if (args.Length > 0 && args[0].Equals("--self-test", StringComparison.OrdinalIgnoreCase))
        {
            SelfTest();
            return;
        }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, Texts.Get("Title", Texts.Detect()), MessageBoxButtons.OK, MessageBoxIcon.Error);
        };
        Application.Run(new MainForm(Texts.Detect()));
    }

    static void RunElevated(string manifest, UiLanguage language)
    {
        try
        {
            string[] paths = File.ReadAllLines(manifest, Encoding.UTF8).Where(File.Exists).ToArray();
            InstallResult result = FontInstaller.Install(paths, true);
            MessageBox.Show(result.ToMessage(language, true), Texts.Get("Title", language), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Texts.Get("Title", language), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { TryDelete(manifest); }
    }

    static void SelfTest()
    {
        if (!FontDiscovery.IsFont("a.otf") || !FontDiscovery.IsFont("b.TTF") || FontDiscovery.IsFont("c.txt")) throw new Exception("Font extension test failed.");
        string root = Path.Combine(Path.GetTempPath(), "FontDropTest");
        Directory.CreateDirectory(root);
        if (!FontDiscovery.SafeExtractPath(root, "a/b.otf").StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase)) throw new Exception("Safe path test failed.");
        try { FontDiscovery.SafeExtractPath(root, "../escape.otf"); throw new Exception("ZIP traversal test failed."); } catch (InvalidDataException) { }
        Console.WriteLine("Self-test passed");
    }

    static void TryDelete(string path) { try { if (File.Exists(path)) File.Delete(path); } catch { } }
}
