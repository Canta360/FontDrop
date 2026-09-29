# Privacy

FontDrop works entirely on your PC. It does not use the network at all: no
account, no telemetry and no update check, and the fonts you open are never
sent anywhere.

## What FontDrop writes, and where

| Location | What is in it |
| --- | --- |
| `%TEMP%\FontDrop\<session>` | Fonts taken out of archives so that they can be previewed and installed. Removed when you press **Clear** or close FontDrop; anything left behind, for example after a crash, is removed the next time FontDrop starts if it is more than two days old. |
| `%TEMP%\FontDrop-<id>.txt` | Only when you install for all users: the list of checked fonts handed to the administrator step. Removed as soon as that step finishes. |
| `%LOCALAPPDATA%\Microsoft\Windows\Fonts` and `HKEY_CURRENT_USER\Software\Microsoft\Windows NT\CurrentVersion\Fonts` | The fonts you install for yourself, registered the same way Windows does. |
| `%WINDIR%\Fonts` and `HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Fonts` | The fonts you install for all users. |

FontDrop keeps no settings or history. The language follows Windows each time
it starts.

## Your controls

- Fonts you installed with FontDrop are ordinary Windows fonts. Remove them
  in **Settings → Personalization → Fonts**, like any other font.
- FontDrop itself is a single program with nothing installed; deleting
  `FontDrop.exe` removes it.

## Other software

To open archives other than ZIP, FontDrop runs the `tar.exe` that comes with
Windows 11 on your PC. It reads the archive locally and does not use the
network either.

---

## プライバシー（日本語）

FontDrop の処理はすべて PC の中で行われます。ネットワークは一切使いません。アカウント、テレメトリ、更新確認はなく、開いたフォントをどこかに送ることもありません。

- 圧縮ファイルから取り出したフォントは、プレビューとインストールのために `%TEMP%\FontDrop\<セッション>` に置きます。「クリア」を押すか FontDrop を閉じると消え、異常終了などで残ったものも、2 日以上たっていれば次の起動時に消します。
- 全ユーザー用にインストールするときだけ、チェックしたフォントの一覧を `%TEMP%\FontDrop-<id>.txt` に書いて管理者権限の処理に渡し、終わったらすぐ消します。
- インストールしたフォントは、Windows と同じ場所と方法で登録します。自分用は `%LOCALAPPDATA%\Microsoft\Windows\Fonts` と `HKEY_CURRENT_USER` の Fonts キー、全ユーザー用は `%WINDIR%\Fonts` と `HKEY_LOCAL_MACHINE` の Fonts キーです。
- 設定や履歴は保存しません。表示言語は起動のたびに Windows に合わせます。
- インストールしたフォントは普通の Windows のフォントなので、「設定 → 個人用設定 → フォント」から削除できます。FontDrop 自体はインストール不要のひとつのファイルなので、`FontDrop.exe` を消せば削除できます。
- ZIP 以外の圧縮ファイルは、Windows 11 に付いている `tar.exe` を PC の中で動かして開きます。これもネットワークは使いません。
