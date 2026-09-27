# FontDrop

[![Build](https://github.com/Canta360/FontDrop/actions/workflows/build.yml/badge.svg)](https://github.com/Canta360/FontDrop/actions/workflows/build.yml)

Windows 用の、フォントをまとめて確認・インストールする小さなツールです。
A small Windows utility for reviewing and batch-installing fonts. ([English below](#english))

![FontDrop](docs/screenshot.png)

## 特長

- フォントファイル・ZIP・フォルダ（サブフォルダ含む）をドラッグ＆ドロップで追加
- 選んだフォントをその場でプレビュー（サンプル文字列は自由に変更可）
- チェックを入れたフォントだけをインストール
- インストール済み（同名ファイル・同名登録）のフォントは自動でスキップ
- 既定は「現在のユーザー用」（管理者権限不要）。チェックを入れると管理者権限で「全ユーザー用」にインストール
- 日本語 / 英語 UI（初期言語は Windows の表示言語に合わせ、アプリ内で切り替え可能）
- 追加のランタイムやインストール不要の単体 exe

対応形式: `.ttf` `.otf` `.ttc` `.fon` `.fnt`

## ダウンロード

[Releases](https://github.com/Canta360/FontDrop/releases) から `FontDrop.exe` をダウンロードして実行してください。
Windows 10 / 11（.NET Framework 4.x は標準搭載）で動作します。

> 署名していない exe のため、初回起動時に SmartScreen の警告が出ることがあります。「詳細情報」→「実行」で起動できます。

## 使い方

1. `FontDrop.exe` を起動する
2. フォントファイル・ZIP・フォルダをウィンドウにドロップ（または「ファイル / ZIP...」「フォルダ...」から選択）
3. 一覧でフォントを選ぶとプレビューが表示されるので、不要なものはチェックを外す
4. 「インストール」を押す

全ユーザー用にインストールしたい場合は「管理者権限で全ユーザー用にインストール」にチェックを入れてから実行します（UAC の確認が表示されます）。

## ビルド

外部パッケージや .NET SDK は不要で、Windows 標準の C# コンパイラ（.NET Framework 4.x の `csc.exe`）でビルドできます。

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

`bin\FontDrop.exe` が生成され、続けてセルフテスト（`FontDrop.exe --self-test`）が実行されます。
アイコンを作り直す場合は `tools\make-icon.ps1` を実行してください。

### リリース

`v1.2.3` のようなタグを push すると、GitHub Actions がビルドして Release に `FontDrop.exe` を添付します。

---

## English

FontDrop is a small Windows utility for reviewing and batch-installing fonts.

- Accepts individual font files, ZIP archives, and folders (including subfolders) via drag and drop
- Shows a live preview with editable sample text
- Installs only the fonts you check
- Skips fonts already installed for the selected scope
- Installs for the current user by default (no admin rights needed); optional elevated all-users mode
- Japanese and English UI; the initial language follows Windows and can be switched in the app
- Single self-contained exe, no runtime to install

Supported formats: `.ttf`, `.otf`, `.ttc`, `.fon`, `.fnt`.

### Download

Get `FontDrop.exe` from [Releases](https://github.com/Canta360/FontDrop/releases). Runs on Windows 10 / 11 (.NET Framework 4.x is built in).
The exe is unsigned, so SmartScreen may warn on first launch — choose "More info" → "Run anyway".

### Build

No SDK or third-party packages are required; the build uses the .NET Framework C# compiler that ships with Windows.

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

This produces `bin\FontDrop.exe` and runs the built-in self-test. Pushing a `v*` tag builds the exe in GitHub Actions and attaches it to a release.

## License

[MIT](LICENSE)
