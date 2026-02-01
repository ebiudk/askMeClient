# AskMe! クライアント セットアップガイド

## システム要件

- Windows 10/11
- .NET 9.0 以上（ランタイムは自動ダウンロード）
- VRChat インストール済み

## インストール手順

### ステップ1: .NET SDKのインストール（開発者向け）

ソースからビルドする場合、以下をインストール：
- [Visual Studio 2022 Community](https://visualstudio.microsoft.com/ja/downloads/)
  - 「ASP.NET と web 開発」ワークロードを選択
  - または .NET デスクトップ開発ワークロード

OR

- [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

### ステップ2: リポジトリのダウンロード

```bash
git clone https://github.com/yourusername/askMe.git
cd askMe/askMeWindows
```

### ステップ3: ビルドと実行

#### オプションA: コマンドラインでビルド

```bash
dotnet build -c Release
dotnet run --configuration Release
```

ビルド済みEXEは以下の場所に出力されます：
```
bin/Release/net9.0-windows/askMeWindows.exe
```

#### オプションB: Visual Studio でビルド

1. Visual Studio 2022 を起動
2. `askMe/askMeWindows/askMeWindows.csproj` を開く
3. メニュー → ビルド → ソリューションのビルド
4. F5 キーで実行

### ステップ4: ショートカット作成（オプション）

EXEを右クリック → 「ショートカットを送る」 → 「デスクトップ」
- デスクトップにショートカットが作成されます

OR スタートメニューにピン留め：
- EXEを右クリック → 「スタートメニューにピン留めする」

## ビルド環境のセットアップ

### Visual Studio Code + .NET CLI

```bash
# .NET 8.0 SDKのインストール確認
dotnet --version

# プロジェクトの復元
dotnet restore

# ビルド
dotnet build

# デバッグ実行
dotnet run

# リリースビルド
dotnet publish -c Release -o publish/
```

### Visual Studio 2022

1. Visual Studio Installer で以下をインストール：
   - ワークロード: ASP.NET と web 開発
   - 個別コンポーネント: .NET 9.0 SDK

2. `askMeWindows.sln` を開く

3. `Build` → `Build Solution` (Ctrl+Shift+B)

4. `Debug` → `Start Without Debugging` (Ctrl+F5)

## トラブルシューティング

### エラー: ".NET SDKが見つかりません"

```bash
# インストール確認
dotnet --version

# 見つからない場合はインストール
# https://dotnet.microsoft.com/en-us/download/dotnet/9.0
```

### エラー: "プロジェクトのビルドに失敗しました"

```bash
# キャッシュクリア
dotnet clean

# 再度復元とビルド
dotnet restore
dotnet build
```

### Windows Defender/SmartScreen警告

初回起動時に警告が表示される場合：
1. 「詳細情報」をクリック
2. 「実行」をクリック

以降は警告が表示されなくなります。

## 開発者向け情報

### プロジェクト構造

```
askMeWindows/
├── Models/              # データモデル（InstanceInfo）
├── Services/            # ビジネスロジック
│   ├── InstanceParser
│   ├── LogTailer
│   ├── LogFileFinder
│   └── VRChatLogMonitorService
├── Views/               # XAML UI
├── ViewModels/          # UI ロジック（MVVM）
└── askMeWindows.csproj  # プロジェクト定義
```

### ビルド設定

- **ターゲットフレームワーク**: net9.0-windows
- **言語バージョン**: Latest C#
- **Nullable**: enabled (null安全性)

### NuGetパッケージ

- `System.Reactive` (v5.4.1) - リアクティブ拡張機能用

### デバッグ実行

```bash
# デバッグビルド
dotnet run

# リリースビルド
dotnet run --configuration Release
```

### コード規約

- C# 10+ の機能を使用
- MVVM パターンを採用
- SRP（単一責務の原則）に従う
- async/await で非同期処理を実装

## リリースの作成

```bash
# リリースビルド
dotnet publish -c Release -o ./release/

# 実行可能ファイル
./release/askMeWindows.exe
```

出力されたファイル：
- `askMeWindows.exe` - メインアプリケーション
- `*.dll` - 依存アセンブリ
- `*.runtimeconfig.json` - ランタイム設定

## CI/CD（GitHub Actions）

プロジェクトに `.github/workflows/` を追加することで自動ビルドが可能です。

## サポート

問題が発生した場合：
1. README.md のトラブルシューティング を確認
2. GitHub Issues で既知の問題を確認
3. 新しいIssueを報告

## 更新

```bash
# リポジトリ更新
git pull origin main

# 新しいバージョンの依存関係をインストール
dotnet restore

# 再度ビルド
dotnet build -c Release
```
