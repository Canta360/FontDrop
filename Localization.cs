using System;
using System.Collections.Generic;
using System.Globalization;

internal enum UiLanguage
{
    Japanese,
    English
}

internal static class Texts
{
    static readonly Dictionary<string, string> Japanese = new Dictionary<string, string>
    {
        { "Title", "FontDrop — 一括フォント導入" },
        { "DropHint", "フォントファイル・圧縮ファイル（ZIP・7z・RAR など）・フォルダをここにドロップ" },
        { "DropHint2", "中身を確認して、チェックしたものだけインストールします" },
        { "Files", "ファイル..." },
        { "Folder", "フォルダ..." },
        { "Clear", "クリア" },
        { "SelectAll", "全選択" },
        { "SelectNone", "全解除" },
        { "Count", "フォント: {0} / 選択: {1}" },
        { "Install", "インストール" },
        { "AllUsers", "管理者権限で全ユーザー用にインストール" },
        { "Sample", "Aaあいう漢字 123" },
        { "PreviewPlaceholder", "フォントを選択するとプレビューを表示します" },
        { "NoFonts", "対応フォントがありません: " },
        { "NoSelection", "インストールするフォントにチェックを入れてください。" },
        { "NoInput", "フォントファイル、ZIP、またはフォルダを追加してください。" },
        { "Unsupported", "対応フォントがありません。" },
        { "PreviewError", "プレビュー不可: " },
        { "UserScope", "現在のユーザー用" },
        { "AllUsersScope", "全ユーザー用" },
        { "Completed", "のフォント登録が完了しました。" },
        { "Installed", "インストール" },
        { "Skipped", "スキップ" },
        { "Failed", "失敗" },
        { "AdminCancelled", "管理者権限がキャンセルされました。" },
        { "SelectFolder", "フォントを探すフォルダを選択してください" },
        { "FontFilter", "フォントまたは圧縮ファイル|*.ttf;*.otf;*.ttc;*.fon;*.fnt;*.zip;*.7z;*.rar;*.tar;*.gz;*.tgz;*.bz2;*.tbz2;*.tbz;*.xz;*.txz;*.zst;*.tzst;*.lzh;*.lha;*.cab|すべてのファイル|*.*" },
        { "ArchiveProblems", "開けなかった圧縮ファイルがあります。ほかのフォントは一覧に追加しました。" },
        { "ArchiveTooDeep", "圧縮ファイルの入れ子が深すぎるため開きませんでした。" },
        { "ArchiveFailed", "開けませんでした。パスワード付きか、壊れている可能性があります。" },
        { "TarUnavailable", "ZIP 以外の圧縮ファイルを開くには Windows 11 が必要です。展開してから、フォルダを追加してください。" },
        { "AndMore", "ほか {0} 件" }
    };

    static readonly Dictionary<string, string> English = new Dictionary<string, string>
    {
        { "Title", "FontDrop — Batch Font Installer" },
        { "DropHint", "Drop font files, archives (ZIP, 7z, RAR and more), or folders here" },
        { "DropHint2", "Review the contents, then install only the checked items" },
        { "Files", "Files..." },
        { "Folder", "Folder..." },
        { "Clear", "Clear" },
        { "SelectAll", "Select all" },
        { "SelectNone", "Clear selection" },
        { "Count", "Fonts: {0} / Selected: {1}" },
        { "Install", "Install" },
        { "AllUsers", "Install for all users (administrator)" },
        { "Sample", "Aaあいう漢字 123" },
        { "PreviewPlaceholder", "Select a font to preview it" },
        { "NoFonts", "No supported fonts found: " },
        { "NoSelection", "Check at least one font to install." },
        { "NoInput", "Add a font file, ZIP, or folder first." },
        { "Unsupported", "No supported fonts found." },
        { "PreviewError", "Preview unavailable: " },
        { "UserScope", "Current user" },
        { "AllUsersScope", "All users" },
        { "Completed", "font registration completed." },
        { "Installed", "Installed" },
        { "Skipped", "Skipped" },
        { "Failed", "Failed" },
        { "AdminCancelled", "Administrator permission was cancelled." },
        { "SelectFolder", "Select a folder containing fonts" },
        { "FontFilter", "Fonts or archives|*.ttf;*.otf;*.ttc;*.fon;*.fnt;*.zip;*.7z;*.rar;*.tar;*.gz;*.tgz;*.bz2;*.tbz2;*.tbz;*.xz;*.txz;*.zst;*.tzst;*.lzh;*.lha;*.cab|All files|*.*" },
        { "ArchiveProblems", "Some archives could not be opened. The other fonts were added to the list." },
        { "ArchiveTooDeep", "Not opened: archives are nested too deeply." },
        { "ArchiveFailed", "Could not open it. It may be password-protected or damaged." },
        { "TarUnavailable", "Archives other than ZIP need Windows 11. Extract it, then add the folder." },
        { "AndMore", "and {0} more" }
    };

    public static string Get(string key, UiLanguage language)
    {
        string value;
        Dictionary<string, string> table = language == UiLanguage.Japanese ? Japanese : English;
        return table.TryGetValue(key, out value) ? value : key;
    }

    public static UiLanguage Detect()
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ja", StringComparison.OrdinalIgnoreCase)
            ? UiLanguage.Japanese : UiLanguage.English;
    }

    public static UiLanguage Parse(string value)
    {
        return String.Equals(value, "ja", StringComparison.OrdinalIgnoreCase) ? UiLanguage.Japanese : UiLanguage.English;
    }

    public static string Code(UiLanguage language)
    {
        return language == UiLanguage.Japanese ? "ja" : "en";
    }
}
