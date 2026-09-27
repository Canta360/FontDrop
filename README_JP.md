<p align="center">
  <img src="docs/images/icon.png" alt="FontDrop" width="96">
</p>

<h1 align="center">FontDrop</h1>

<p align="center">
  Windows 用の、フォントをまとめてインストールするツール。<br>
  フォントや ZIP、フォルダをまるごとドロップして、見た目を確かめてから、必要なものだけをインストールできます。
</p>

<p align="center">
  <a href="README.md">English</a> ·
  <a href="#インストール">インストール</a> ·
  <a href="#ビルド">ビルド</a> ·
  <a href="https://capitata.dev">capitata.dev</a>
</p>

<p align="center">
  <img src="docs/images/screenshot-ja.png" alt="8 個のフォントを一覧に表示し、游ゴシックをプレビューしている FontDrop" width="640">
</p>

## 使い方

1. フォントファイル、ZIP、フォルダをウィンドウにドロップします。
   「ファイル / ZIP...」「フォルダ...」から選ぶこともできます。フォルダはサブフォルダまで探します。
2. フォントをクリックするとプレビューが表示されます。上の欄でサンプルの文字を変えられます。
3. いらないもののチェックを外して、「インストール」を押します。

インストール先は現在のユーザーなので、管理者権限はいりません。PC のすべてのユーザーに入れたいときは、
先に「管理者権限で全ユーザー用にインストール」にチェックを入れてください。コピーの前に Windows が確認を求めます。
すでにインストールされているフォントはスキップします。

対応形式: `.ttf`、`.otf`、`.ttc`、`.fon`、`.fnt`

画面は Windows の表示言語に合わせて日本語か英語で表示され、右上のメニューで切り替えられます。

## インストール

[Releases](https://github.com/Canta360/FontDrop/releases/latest) から `FontDrop-1.0.0.exe` をダウンロードして実行してください。
インストールは不要で、ひとつのファイルだけで動きます。Windows 10 / 11 には必要な .NET Framework が最初から入っています。

まだコード署名をしていないため、初回の起動時に Windows SmartScreen が確認を求めることがあります。

## ビルド

SDK やパッケージは不要で、Windows の .NET Framework に付いている C# コンパイラでビルドします。

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

`bin\FontDrop.exe` ができ、続けてセルフテスト（`FontDrop.exe --self-test`）が実行されます。
アイコンは `tools\make-icon.ps1` で描き直せます。

`v*` のタグを push すると、GitHub Actions がビルドし、`.github/release-notes/<タグ>.md` のノートと
SHA-256 チェックサムを付けてリリースを公開します。

## ライセンス

FontDrop は [MIT License](LICENSE) で公開しています。

FontDrop は [Capitata](https://capitata.dev) が作っています。

<p align="center">
  <a href="https://capitata.dev">
    <picture>
      <source media="(prefers-color-scheme: dark)" srcset="docs/images/capitata-dark.png">
      <img src="docs/images/capitata-light.png" alt="Capitata" width="160">
    </picture>
  </a>
</p>
