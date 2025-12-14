# TextToSpeech (.NET)

This repository provides bilingual documentation. Jump to your preferred language:
- [日本語ガイド](#日本語ガイド)
- [English Guide](#english-guide)

## 日本語ガイド

Azure Speech を利用して台本を音声化する .NET 8 コンソールアプリです。役名ごとに Azure Neural Voice を割り当て、SSML を生成しながら行単位で音声を合成・ミックスして 1 本の音声トラックを出力します。

### 機能概要
- JSON 台本 (`lineId`, `speaker`, `emotion`, `text`, 任意 `pauseMs`) の読み込み。
- YAML ボイス設定から話者ごとのボイス・話速・ピッチなどを解決。
- Azure Speech Service で行ごとに WAV を合成し、`AudioMixer` で連結。
- Windows Media Foundation を用いて MP3 にもエクスポート、行ごとの WAV も保持。
- 合成内容を JSON Lines 形式で `logs/` ディレクトリへ記録。

### 準備
- .NET SDK 8.0 以降
- Azure Speech Service リソース (キーとリージョン)
- Windows Media Foundation (Windows では既定で有効、MP3 エクスポートに使用)

#### 依存パッケージ
`src/TextToSpeech/TextToSpeech.csproj` では次の NuGet パッケージを使用しています。
- `Microsoft.CognitiveServices.Speech` – Azure Speech SDK
- `YamlDotNet` – `config/voices.yaml` の読み込み
- `DotNetEnv` – `.env` から環境変数を取得
- `NAudio` – WAV の連結と MP3 変換

### 設定
- `.env.sample` を `.env` にコピーし、Azure Speech の資格情報を記入します。
  ```powershell
  Copy-Item .env.sample .env
  # AZURE_SPEECH_KEY / AZURE_SPEECH_REGION を設定
  ```
- `config/voices.yaml`
  - `voices`: 役名→ボイス設定 (style, prosody, file_tag など)
  - `aliases`: ニックネームや別名を正規化するマップ
  - `defaults`: 出力フォーマット、デフォルト休止時間、フォールバック音声
- `data/script_scene1.json`
  - `lineId`, `speaker`, `emotion`, `text`, `pauseMs` を持つ配列

必要に応じて `OUTPUT_AUDIO_FORMAT` 環境変数を設定すると、`--out` で拡張子を省略した場合のエクスポート形式を上書きできます (例: `mp3` / `wav`)。

### ビルド
```powershell
dotnet restore TextToSpeech.sln
dotnet build TextToSpeech.sln
```

### 実行
```powershell
dotnet run --project src/TextToSpeech -- \
  --script data/script_scene1.json \
  --voices config/voices.yaml \
  --out output/scene1/scene1_final.mp3 \
  --log-dir logs
```

#### CLI オプション
- `--script` (既定: `data/script_scene1.json`) – 台本 JSON のパス。
- `--voices` (既定: `config/voices.yaml`) – ボイス設定 YAML のパス。
- `--out` (既定: `output/scene1/scene1_final.mp3`) – 最終出力ファイルのパス。
- `--log-dir` (既定: `logs`) – 合成ログ (`synth_<sceneId>.jsonl`) の出力先。
- `--scene-id` – ログや一時ディレクトリ名に利用。省略時は台本ファイル名を使用。

### 出力
- 最終音声: `--out` で指定したパスに MP3 / WAV を生成。
- 行ごとの WAV: `--out` のディレクトリ配下に `<sceneId>/<lineId>_<fileTag>.wav` を保存。
- 合成ログ: `--log-dir` 配下の `synth_<sceneId>.jsonl` に 1 行 1 JSON でメタデータを追記 (ディレクトリは自動生成)。

### プロジェクト構成 (2025-12-05 現在)
```
.
├── TextToSpeech.sln
├── .env.sample
├── README.md
├── config/
│   └── voices.yaml
├── data/
│   └── script_scene1.json
├── output/
│   └── (生成された音声を配置)
└── src/TextToSpeech/
    ├── TextToSpeech.csproj
    ├── CliOptions.cs
    ├── Configuration/
    │   └── VoiceConfiguration.cs
    ├── Models/
    │   ├── DialogueLine.cs
    │   ├── RenderedLine.cs
    │   ├── SynthesisLogEntry.cs
    │   └── VoiceProfile.cs
    └── Services/
        ├── AudioExporter.cs
        ├── AudioMixer.cs
        ├── SpeechSynthesizerService.cs
        ├── SsmlBuilder.cs
        └── WaveConcatenator.cs
```

## English Guide

This .NET 8 console app turns a script into speech with Azure Speech. Each character can use a dedicated Azure Neural Voice. The app builds SSML per line, synthesizes individual clips, and mixes them into a single track.

### Features
- Loads a JSON script (`lineId`, `speaker`, `emotion`, `text`, optional `pauseMs`).
- Resolves voice, speaking rate, pitch, and more from a YAML configuration.
- Synthesizes WAV files line by line with Azure Speech Service and stitches them using `AudioMixer`.
- Uses Windows Media Foundation to export MP3 while keeping the per-line WAV files.
- Logs synthesis metadata as JSON Lines under `logs/`.

### Prerequisites
- .NET SDK 8.0 or later
- Azure Speech Service resource (key and region)
- Windows Media Foundation (enabled by default on Windows for MP3 export)

#### Dependencies
`src/TextToSpeech/TextToSpeech.csproj` references these NuGet packages:
- `Microsoft.CognitiveServices.Speech` – Azure Speech SDK
- `YamlDotNet` – YAML parser for `config/voices.yaml`
- `DotNetEnv` – Loads environment variables from `.env`
- `NAudio` – WAV concatenation and MP3 conversion

### Configuration
- Copy `.env.sample` to `.env` and add your Azure Speech credentials.
  ```powershell
  Copy-Item .env.sample .env
  # Set AZURE_SPEECH_KEY / AZURE_SPEECH_REGION
  ```
- `config/voices.yaml`
  - `voices`: character-to-voice mapping (style, prosody, file_tag, etc.)
  - `aliases`: normalizes nicknames and alternate spellings
  - `defaults`: default output format, pause duration, fallback voice
- `data/script_scene1.json`
  - Array of entries containing `lineId`, `speaker`, `emotion`, `text`, `pauseMs`

Set the `OUTPUT_AUDIO_FORMAT` environment variable to override the export format when the `--out` path omits an extension (`mp3` or `wav`).

### Build
```powershell
dotnet restore TextToSpeech.sln
dotnet build TextToSpeech.sln
```

### Run
```powershell
dotnet run --project src/TextToSpeech -- \
  --script data/script_scene1.json \
  --voices config/voices.yaml \
  --out output/scene1/scene1_final.mp3 \
  --log-dir logs
```

#### CLI Options
- `--script` (default: `data/script_scene1.json`) – Path to the script JSON file.
- `--voices` (default: `config/voices.yaml`) – Path to the voice configuration YAML.
- `--out` (default: `output/scene1/scene1_final.mp3`) – Destination for the final audio file.
- `--log-dir` (default: `logs`) – Directory for synthesis logs (`synth_<sceneId>.jsonl`).
- `--scene-id` – Identifier used in logs and temp directories; defaults to the script filename.

### Output
- Final audio: MP3 or WAV saved at the `--out` path.
- Per-line WAV files: stored under `--out` directory as `<sceneId>/<lineId>_<fileTag>.wav`.
- Synthesis log: metadata appended to `--log-dir/synth_<sceneId>.jsonl` (directory created automatically).

### Project Layout (as of 2025-12-05)
```
.
├── TextToSpeech.sln
├── .env.sample
├── README.md
├── config/
│   └── voices.yaml
├── data/
│   └── script_scene1.json
├── output/
│   └── (generated audio lives here)
└── src/TextToSpeech/
    ├── TextToSpeech.csproj
    ├── CliOptions.cs
    ├── Configuration/
    │   └── VoiceConfiguration.cs
    ├── Models/
    │   ├── DialogueLine.cs
    │   ├── RenderedLine.cs
    │   ├── SynthesisLogEntry.cs
    │   └── VoiceProfile.cs
    └── Services/
        ├── AudioExporter.cs
        ├── AudioMixer.cs
        ├── SpeechSynthesizerService.cs
        ├── SsmlBuilder.cs
        └── WaveConcatenator.cs
```


