using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

internal sealed class MainForm : Form
{
    readonly CheckedListBox fontList = new CheckedListBox();
    readonly List<FontEntry> entries = new List<FontEntry>();
    readonly HashSet<string> addedSources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    readonly Label hint = new Label();
    readonly Label preview = new Label();
    readonly Label previewInfo = new Label();
    readonly Label listStatus = new Label();
    readonly TextBox sample = new TextBox();
    readonly ComboBox languagePicker = new ComboBox();
    readonly CheckBox allUsers = new CheckBox();
    readonly Button addFiles = new Button();
    readonly Button addFolder = new Button();
    readonly Button clear = new Button();
    readonly Button selectAll = new Button();
    readonly Button selectNone = new Button();
    readonly Button install = new Button();
    PrivateFontCollection previewFonts = new PrivateFontCollection();
    Font previewFont;
    UiLanguage language;
    bool changingLanguage;
    bool handedOffToElevated;

    public MainForm(UiLanguage initialLanguage)
    {
        language = initialLanguage;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = SystemFonts.MessageBoxFont;
        Text = Texts.Get("Title", language);
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
        Width = 820;
        Height = 520;
        MinimumSize = new Size(680, 420);
        StartPosition = FormStartPosition.CenterScreen;
        AllowDrop = true;
        DragEnter += OnDragEnter;
        DragDrop += OnDragDrop;
        BuildLayout();
        ApplyLanguage();
        FormClosed += delegate
        {
            DisposePreview();
            if (!handedOffToElevated) FontDiscovery.DeleteSessionFolder(FontDiscovery.SessionTemp);
        };
    }

    void BuildLayout()
    {
        Panel header = new Panel { Dock = DockStyle.Top, Height = 82, Padding = new Padding(12, 8, 12, 4) };
        hint.Dock = DockStyle.Fill;
        hint.TextAlign = ContentAlignment.MiddleLeft;
        hint.Font = new Font(Font.FontFamily, 11);
        languagePicker.Width = 100;
        languagePicker.DropDownStyle = ComboBoxStyle.DropDownList;
        languagePicker.Items.Add("日本語");
        languagePicker.Items.Add("English");
        languagePicker.Dock = DockStyle.Right;
        languagePicker.SelectedIndexChanged += delegate
        {
            if (changingLanguage) return;
            language = languagePicker.SelectedIndex == 0 ? UiLanguage.Japanese : UiLanguage.English;
            ApplyLanguage();
        };
        header.Controls.Add(hint);
        header.Controls.Add(languagePicker);

        fontList.Dock = DockStyle.Fill;
        fontList.CheckOnClick = true;
        fontList.AllowDrop = true;
        fontList.DragEnter += OnDragEnter;
        fontList.DragDrop += OnDragDrop;
        fontList.SelectedIndexChanged += delegate { UpdatePreview(); };
        fontList.ItemCheck += delegate { BeginInvoke((Action)UpdateSelectionStatus); };
        fontList.HorizontalScrollbar = true;
        selectAll.Click += delegate { SetAllChecked(true); };
        selectNone.Click += delegate { SetAllChecked(false); };
        selectAll.Width = 78;
        selectNone.Width = 92;
        listStatus.Dock = DockStyle.Fill;
        listStatus.TextAlign = ContentAlignment.MiddleRight;
        Panel listFooter = new Panel { Dock = DockStyle.Bottom, Height = 34, Padding = new Padding(4, 3, 4, 3) };
        FlowLayoutPanel listActions = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 180, WrapContents = false };
        listActions.Controls.Add(selectAll);
        listActions.Controls.Add(selectNone);
        listFooter.Controls.Add(listStatus);
        listFooter.Controls.Add(listActions);
        Panel listPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        listPanel.Controls.Add(fontList);
        listPanel.Controls.Add(listFooter);

        sample.Dock = DockStyle.Top;
        sample.Height = 28;
        sample.TextChanged += delegate { preview.Text = sample.Text; };
        preview.Dock = DockStyle.Fill;
        preview.TextAlign = ContentAlignment.MiddleCenter;
        preview.Padding = new Padding(12);
        preview.AutoEllipsis = true;
        previewInfo.Dock = DockStyle.Bottom;
        previewInfo.Height = 46;
        previewInfo.TextAlign = ContentAlignment.MiddleCenter;
        Panel previewPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        previewPanel.Controls.Add(preview);
        previewPanel.Controls.Add(previewInfo);
        previewPanel.Controls.Add(sample);
        SplitContainer split = new SplitContainer { Dock = DockStyle.Fill };
        Load += delegate { split.SplitterDistance = split.Width / 2; };
        split.Panel1.Controls.Add(listPanel);
        split.Panel2.Controls.Add(previewPanel);

        addFiles.Width = 108;
        addFolder.Width = 88;
        clear.Width = 70;
        install.Width = 105;
        addFiles.Click += delegate { SelectFiles(); };
        addFolder.Click += delegate { SelectFolder(); };
        clear.Click += delegate { ClearList(); };
        install.Click += delegate { StartInstall(); };
        allUsers.AutoSize = true;
        FlowLayoutPanel footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 46, Padding = new Padding(10, 7, 10, 5) };
        footer.Controls.Add(addFiles);
        footer.Controls.Add(addFolder);
        footer.Controls.Add(clear);
        footer.Controls.Add(allUsers);
        footer.Controls.Add(install);

        Controls.Add(split);
        Controls.Add(header);
        Controls.Add(footer);
    }

    void ApplyLanguage()
    {
        Text = Texts.Get("Title", language);
        hint.Text = Texts.Get("DropHint", language) + "\r\n" + Texts.Get("DropHint2", language);
        addFiles.Text = Texts.Get("Files", language);
        addFolder.Text = Texts.Get("Folder", language);
        clear.Text = Texts.Get("Clear", language);
        selectAll.Text = Texts.Get("SelectAll", language);
        selectNone.Text = Texts.Get("SelectNone", language);
        install.Text = Texts.Get("Install", language);
        allUsers.Text = Texts.Get("AllUsers", language);
        if (String.IsNullOrEmpty(sample.Text) || sample.Text == Texts.Get("Sample", language == UiLanguage.Japanese ? UiLanguage.English : UiLanguage.Japanese))
            sample.Text = Texts.Get("Sample", language);
        if (languagePicker.SelectedIndex != (language == UiLanguage.Japanese ? 0 : 1))
        {
            changingLanguage = true;
            languagePicker.SelectedIndex = language == UiLanguage.Japanese ? 0 : 1;
            changingLanguage = false;
        }
        UpdatePreview();
        UpdateSelectionStatus();
    }

    void SelectFiles()
    {
        using (OpenFileDialog dialog = new OpenFileDialog { Multiselect = true, Filter = Texts.Get("FontFilter", language) })
            if (dialog.ShowDialog(this) == DialogResult.OK) AddSources(dialog.FileNames);
    }

    void SelectFolder()
    {
        using (FolderBrowserDialog dialog = new FolderBrowserDialog { Description = Texts.Get("SelectFolder", language) })
            if (dialog.ShowDialog(this) == DialogResult.OK) AddSources(new[] { dialog.SelectedPath });
    }

    void OnDragEnter(object sender, DragEventArgs e)
    {
        e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    void OnDragDrop(object sender, DragEventArgs e)
    {
        AddSources((string[])e.Data.GetData(DataFormats.FileDrop));
    }

    void AddSources(IEnumerable<string> paths)
    {
        foreach (string path in paths)
        {
            if (!File.Exists(path) && !Directory.Exists(path)) continue;
            string source = Path.GetFullPath(path);
            if (!addedSources.Add(source)) continue;
            try
            {
                List<FontEntry> found = FontDiscovery.Inspect(source);
                foreach (FontEntry entry in found)
                {
                    if (entries.Any(item => item.SourcePath.Equals(entry.SourcePath, StringComparison.OrdinalIgnoreCase))) continue;
                    entries.Add(entry);
                    fontList.Items.Add(entry, true);
                }
                UpdateSelectionStatus();
                if (found.Count == 0) MessageBox.Show(this, Texts.Get("NoFonts", language) + Path.GetFileName(source), Texts.Get("Title", language));
            }
            catch (Exception ex)
            {
                addedSources.Remove(source);
                MessageBox.Show(this, ex.Message, Texts.Get("Title", language), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    void ClearList()
    {
        entries.Clear();
        addedSources.Clear();
        fontList.Items.Clear();
        UpdatePreview();
        UpdateSelectionStatus();
        FontDiscovery.DeleteSessionFolder(FontDiscovery.SessionTemp);
    }

    void SetAllChecked(bool value)
    {
        for (int i = 0; i < fontList.Items.Count; i++) fontList.SetItemChecked(i, value);
        UpdateSelectionStatus();
    }

    void UpdateSelectionStatus()
    {
        listStatus.Text = String.Format(Texts.Get("Count", language), entries.Count, fontList.CheckedItems.Count);
    }

    void UpdatePreview()
    {
        DisposePreview();
        int index = fontList.SelectedIndex;
        if (index < 0 || index >= entries.Count)
        {
            preview.Font = new Font(Font.FontFamily, 24);
            previewInfo.Text = Texts.Get("PreviewPlaceholder", language);
            return;
        }
        try
        {
            FontEntry entry = entries[index];
            previewFonts = new PrivateFontCollection();
            previewFonts.AddFontFile(entry.SourcePath);
            if (previewFonts.Families.Length == 0) throw new InvalidDataException("No font family found.");
            previewFont = new Font(previewFonts.Families[0], 26, FontStyle.Regular);
            preview.Font = previewFont;
            previewInfo.Text = entry.DisplayName;
        }
        catch (Exception ex)
        {
            previewInfo.Text = Texts.Get("PreviewError", language) + ex.Message;
        }
    }

    void DisposePreview()
    {
        if (previewFont != null) { previewFont.Dispose(); previewFont = null; }
        if (previewFonts != null) { previewFonts.Dispose(); previewFonts = new PrivateFontCollection(); }
    }

    void StartInstall()
    {
        string[] selected = entries.Where((entry, index) => fontList.GetItemChecked(index)).Select(entry => entry.SourcePath).ToArray();
        if (selected.Length == 0) { MessageBox.Show(this, Texts.Get("NoSelection", language), Texts.Get("Title", language)); return; }
        if (allUsers.Checked) { StartElevated(selected); return; }
        SetBusy(true);
        Task.Factory.StartNew<InstallResult>(delegate { return FontInstaller.Install(selected, false); }).ContinueWith(task =>
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                BeginInvoke((Action)delegate
                {
                    SetBusy(false);
                    if (task.IsFaulted) ShowError(task.Exception.GetBaseException().Message);
                    else ShowResult(task.Result, false);
                });
            }
            catch (InvalidOperationException) { }
        });
    }

    void StartElevated(string[] selected)
    {
        string manifest = Path.Combine(Path.GetTempPath(), "FontDrop-" + Guid.NewGuid().ToString("N") + ".txt");
        try
        {
            File.WriteAllLines(manifest, selected, Encoding.UTF8);
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(Application.ExecutablePath, "--elevated \"" + manifest + "\" " + Texts.Code(language) + " \"" + FontDiscovery.SessionTemp + "\"") { Verb = "runas", UseShellExecute = true };
            System.Diagnostics.Process.Start(info);
            handedOffToElevated = true;
            Close();
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            TryDelete(manifest);
            if (ex.NativeErrorCode == 1223) MessageBox.Show(this, Texts.Get("AdminCancelled", language), Texts.Get("Title", language));
            else ShowError(ex.Message);
        }
    }

    void SetBusy(bool busy)
    {
        addFiles.Enabled = !busy;
        addFolder.Enabled = !busy;
        clear.Enabled = !busy;
        selectAll.Enabled = !busy;
        selectNone.Enabled = !busy;
        install.Enabled = !busy;
        allUsers.Enabled = !busy;
        languagePicker.Enabled = !busy;
    }

    void ShowResult(InstallResult result, bool allUsersScope)
    {
        MessageBox.Show(this, result.ToMessage(language, allUsersScope), Texts.Get("Title", language), MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    void ShowError(string message)
    {
        MessageBox.Show(this, message, Texts.Get("Title", language), MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    static void TryDelete(string path) { try { if (File.Exists(path)) File.Delete(path); } catch { } }
}
